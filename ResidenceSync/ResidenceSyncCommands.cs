using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;


namespace ResidenceSync
{
    public class ResidenceSyncCommands
    {
        private const string MASTER_PATH = @"C:\_CG_SHARED\Master_Residences.dwg";
        private const string RESIDENCE_LAYER = "Z-RESIDENCE";
        private const double LENGTH_TOLERANCE = 1e-9;

        [CommandMethod("ResidenceSync", "PUSHRES", CommandFlags.Modal)]
        public void PushResidences()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            if (!PromptSectionKey(ed, out SectionKey sectionKey))
            {
                return;
            }

            if (!PromptCornerPair(ed, out Point3d sketchTopLeft, out Point3d sketchTopRight))
            {
                return;
            }

            if (!PromptSectionPolyline(ed, out ObjectId sectionId))
            {
                return;
            }

            if (!TryGetSectionCorners(doc.Database, sectionId, out Point3d trueTopLeft, out Point3d trueTopRight))
            {
                ed.WriteMessage("\nPUSHRES: Failed to derive true section corners.");
                return;
            }

            if (!TryBuildSimilarity(trueTopLeft, trueTopRight, sketchTopLeft, sketchTopRight, out _, out Matrix3d sketchToMaster))
            {
                ed.WriteMessage("\nPUSHRES: Invalid similarity transform (check picked corners).");
                return;
            }

            PromptSelectionOptions selOptions = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect residence blocks to push: "
            };
            SelectionFilter filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Start, "INSERT")
            });

            PromptSelectionResult selResult = ed.GetSelection(selOptions, filter);
            if (selResult.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nPUSHRES: No residence blocks selected.");
                return;
            }

            List<Point3d> masterPoints = new List<Point3d>();
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject sel in selResult.Value)
                {
                    if (sel.ObjectId.IsNull)
                    {
                        continue;
                    }

                    BlockReference blockRef = tr.GetObject(sel.ObjectId, OpenMode.ForRead, false) as BlockReference;
                    if (blockRef == null)
                    {
                        continue;
                    }

                    Point3d transformed = blockRef.Position.TransformBy(sketchToMaster);
                    masterPoints.Add(transformed);
                }

                tr.Commit();
            }

            if (masterPoints.Count == 0)
            {
                ed.WriteMessage("\nPUSHRES: Selected blocks had no valid positions.");
                return;
            }

            int appended = AppendPointsToMaster(masterPoints);
            ed.WriteMessage($"\nPUSHRES: Wrote {appended} residence point(s) into master for {sectionKey}.");
        }

        [CommandMethod("ResidenceSync", "PULLRES", CommandFlags.Modal)]
        public void PullResidences()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            if (!PromptSectionKey(ed, out SectionKey sectionKey))
            {
                return;
            }

            if (!PromptCornerPair(ed, out Point3d sketchTopLeft, out Point3d sketchTopRight))
            {
                return;
            }

            if (!PromptSectionPolyline(ed, out ObjectId sectionId))
            {
                return;
            }

            if (!TryGetSectionCorners(doc.Database, sectionId, out Point3d trueTopLeft, out Point3d trueTopRight))
            {
                ed.WriteMessage("\nPULLRES: Failed to derive true section corners.");
                return;
            }

            if (!TryBuildSimilarity(trueTopLeft, trueTopRight, sketchTopLeft, sketchTopRight, out Matrix3d masterToSketch, out Matrix3d sketchToMaster))
            {
                ed.WriteMessage("\nPULLRES: Invalid similarity transform (check picked corners).");
                return;
            }

            if (!BuildSectionAabb(trueTopLeft, trueTopRight, out Aabb2d sectionBox))
            {
                ed.WriteMessage("\nPULLRES: Section extents are degenerate.");
                return;
            }

            bool masterExists;
            List<Point3d> masterPoints = ReadPointsFromMaster(out masterExists);
            if (!masterExists)
            {
                ed.WriteMessage("\nPULLRES: Master drawing not found; nothing to pull.");
                return;
            }

            List<Point3d> sketchPoints = new List<Point3d>();
            foreach (Point3d pt in masterPoints)
            {
                if (IsInsideAabb(sectionBox, pt))
                {
                    Point3d mapped = pt.TransformBy(masterToSketch);
                    sketchPoints.Add(mapped);
                }
            }

            if (sketchPoints.Count == 0)
            {
                ed.WriteMessage("\nPULLRES: No residence point(s) found for the requested section.");
                return;
            }

            using (doc.LockDocument())
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    EnsureLayer(doc.Database, RESIDENCE_LAYER, tr);

                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    foreach (Point3d point in sketchPoints)
                    {
                        DBPoint dbPoint = new DBPoint(point)
                        {
                            Layer = RESIDENCE_LAYER
                        };

                        modelSpace.AppendEntity(dbPoint);
                        tr.AddNewlyCreatedDBObject(dbPoint, true);
                    }
                    EnsurePointStyleVisible();
                    tr.Commit();
                }
            }

            ed.WriteMessage($"\nPULLRES: Inserted {sketchPoints.Count} residence point(s) into this sketch for {sectionKey}.");
        }
        private static void EnsurePointStyleVisible()
        {
            try
            {
                AcadApp.SetSystemVariable("PDMODE", 3);
                AcadApp.SetSystemVariable("PDSIZE", 0.8);
            }
            catch
            {
                // ignore if system vars are locked
            }
        }
        private bool PromptSectionKey(Editor ed, out SectionKey key)
        {
            key = default;

            string sec = PromptString(ed, "Enter SEC: ");
            if (sec == null)
            {
                return false;
            }

            string twp = PromptString(ed, "Enter TWP: ");
            if (twp == null)
            {
                return false;
            }

            string rge = PromptString(ed, "Enter RGE: ");
            if (rge == null)
            {
                return false;
            }

            string mer = PromptString(ed, "Enter MER: ");
            if (mer == null)
            {
                return false;
            }

            key = new SectionKey(sec, twp, rge, mer);
            return true;
        }

        private string PromptString(Editor ed, string message)
        {
            PromptStringOptions opts = new PromptStringOptions("\n" + message)
            {
                AllowSpaces = false
            };
            PromptResult res = ed.GetString(opts);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

            return res.StringResult;
        }

        private bool PromptCornerPair(Editor ed, out Point3d topLeft, out Point3d topRight)
        {
            topLeft = Point3d.Origin;
            topRight = Point3d.Origin;

            PromptPointResult tlRes = ed.GetPoint("\nPick Top-Left on sketch: ");
            if (tlRes.Status != PromptStatus.OK)
            {
                return false;
            }

            PromptPointOptions pro = new PromptPointOptions("\nPick Top-Right on sketch: ")
            {
                BasePoint = tlRes.Value,
                UseBasePoint = true
            };

            PromptPointResult trRes = ed.GetPoint(pro);
            if (trRes.Status != PromptStatus.OK)
            {
                return false;
            }

            if (tlRes.Value.DistanceTo(trRes.Value) < LENGTH_TOLERANCE)
            {
                ed.WriteMessage("\nThe picked Top-Left and Top-Right points are coincident.");
                return false;
            }

            topLeft = tlRes.Value;
            topRight = trRes.Value;
            return true;
        }

        private bool PromptSectionPolyline(Editor ed, out ObjectId polylineId)
        {
            PromptEntityOptions opts = new PromptEntityOptions("\nSelect the actual section polyline: ")
            {
                AllowNone = false
            };
            opts.SetRejectMessage("\nEntity must be a 2D or LW polyline.");
            opts.AddAllowedClass(typeof(Polyline), false);
            opts.AddAllowedClass(typeof(Polyline2d), false);
            opts.AddAllowedClass(typeof(Polyline3d), false);

            PromptEntityResult res = ed.GetEntity(opts);
            if (res.Status != PromptStatus.OK)
            {
                polylineId = ObjectId.Null;
                return false;
            }

            polylineId = res.ObjectId;
            return true;
        }

        private bool TryGetSectionCorners(Database db, ObjectId polylineId, out Point3d topLeft, out Point3d topRight)
        {
            topLeft = Point3d.Origin;
            topRight = Point3d.Origin;

            List<Point3d> vertices = new List<Point3d>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(polylineId, OpenMode.ForRead) as Entity;
                if (ent is Polyline poly)
                {
                    for (int i = 0; i < poly.NumberOfVertices; i++)
                    {
                        vertices.Add(poly.GetPoint3dAt(i));
                    }
                }
                else if (ent is Polyline2d poly2d)
                {
                    foreach (ObjectId vertId in poly2d)
                    {
                        Vertex2d vert = tr.GetObject(vertId, OpenMode.ForRead) as Vertex2d;
                        if (vert != null)
                        {
                            vertices.Add(vert.Position);
                        }
                    }
                }
                else if (ent is Polyline3d poly3d)
                {
                    foreach (ObjectId vertId in poly3d)
                    {
                        PolylineVertex3d vert = tr.GetObject(vertId, OpenMode.ForRead) as PolylineVertex3d;
                        if (vert != null)
                        {
                            vertices.Add(new Point3d(vert.Position.X, vert.Position.Y, 0.0));
                        }
                    }
                }

                tr.Commit();
            }

            if (vertices.Count == 0)
            {
                return false;
            }

            double maxY = vertices.Max(p => p.Y);
            List<Point3d> topCandidates = vertices
                .Where(p => Math.Abs(p.Y - maxY) < 1e-6)
                .OrderBy(p => p.X)
                .ToList();

            if (topCandidates.Count >= 2)
            {
                topLeft = topCandidates.First();
                topRight = topCandidates.Last();
                return true;
            }

            double minX = vertices.Min(p => p.X);
            double maxX = vertices.Max(p => p.X);

            topLeft = new Point3d(minX, maxY, 0.0);
            topRight = new Point3d(maxX, maxY, 0.0);
            return true;
        }

        private bool TryBuildSimilarity(Point3d trueTopLeft, Point3d trueTopRight, Point3d sketchTopLeft, Point3d sketchTopRight, out Matrix3d masterToSketch, out Matrix3d sketchToMaster)
        {
            masterToSketch = Matrix3d.Identity;
            sketchToMaster = Matrix3d.Identity;

            Vector3d vTrue = trueTopRight - trueTopLeft;
            Vector3d vSketch = sketchTopRight - sketchTopLeft;

            double trueLength = vTrue.Length;
            double sketchLength = vSketch.Length;

            if (trueLength < LENGTH_TOLERANCE || sketchLength < LENGTH_TOLERANCE)
            {
                return false;
            }

            double scale = sketchLength / trueLength;
            double angle = vTrue.GetAngleTo(vSketch, Vector3d.ZAxis);

            Matrix3d translateToOrigin = Matrix3d.Displacement(-trueTopLeft.GetAsVector());
            Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
            Matrix3d scaling = Matrix3d.Scaling(scale, Point3d.Origin);
            Matrix3d translateToSketch = Matrix3d.Displacement(sketchTopLeft.GetAsVector());

            masterToSketch = translateToSketch * scaling * rotation * translateToOrigin;
            try
            {
                sketchToMaster = masterToSketch.Inverse();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                masterToSketch = Matrix3d.Identity;
                sketchToMaster = Matrix3d.Identity;
                return false;
            }
            return true;
        }

        private int AppendPointsToMaster(IEnumerable<Point3d> points)
        {
            List<Point3d> pointList = points.Where(p => !double.IsNaN(p.X) && !double.IsNaN(p.Y)).ToList();
            if (pointList.Count == 0)
            {
                return 0;
            }

            string directory = Path.GetDirectoryName(MASTER_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(MASTER_PATH))
            {
                using (Database newDb = new Database(true, true))
                {
                    newDb.SaveAs(MASTER_PATH, DwgVersion.Current);
                }
            }

            int appended = 0;
            using (Database masterDb = new Database(false, true))
            {
                masterDb.ReadDwgFile(MASTER_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                masterDb.CloseInput(true);

                using (Transaction tr = masterDb.TransactionManager.StartTransaction())
                {
                    EnsureLayer(masterDb, RESIDENCE_LAYER, tr);
                    // Resolve the layer ObjectId once, then reuse
                    LayerTable lt = (LayerTable)tr.GetObject(masterDb.LayerTableId, OpenMode.ForRead);
                    ObjectId resLayerId = lt[RESIDENCE_LAYER];

                    BlockTable bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    foreach (Point3d point in pointList)
                    {
                        DBPoint dbPoint = new DBPoint(point);
                        dbPoint.LayerId = resLayerId;          // <-- changed
                        modelSpace.AppendEntity(dbPoint);
                        tr.AddNewlyCreatedDBObject(dbPoint, true);
                        appended++;
                    }


                    tr.Commit();
                }

                masterDb.SaveAs(MASTER_PATH, DwgVersion.Current);
            }

            return appended;
        }

        private List<Point3d> ReadPointsFromMaster(out bool exists)
        {
            exists = File.Exists(MASTER_PATH);
            if (!exists)
            {
                return new List<Point3d>();
            }

            List<Point3d> points = new List<Point3d>();
            using (Database masterDb = new Database(false, true))
            {
                masterDb.ReadDwgFile(MASTER_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                masterDb.CloseInput(true);

                using (Transaction tr = masterDb.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    foreach (ObjectId id in modelSpace)
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent == null)
                        {
                            continue;
                        }

                        switch (ent)
                        {
                            case DBPoint dbPoint:
                                points.Add(dbPoint.Position);
                                break;
                            case BlockReference blockRef:
                                points.Add(blockRef.Position);
                                break;
                        }
                    }

                    tr.Commit();
                }
            }

            return points;
        }

        private void EnsureLayer(Database db, string layerName, Transaction tr)
        {
            LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!layerTable.Has(layerName))
            {
                layerTable.UpgradeOpen();
                LayerTableRecord layerRecord = new LayerTableRecord
                {
                    Name = layerName
                };

                layerTable.Add(layerRecord);
                tr.AddNewlyCreatedDBObject(layerRecord, true);
            }
        }

        private bool BuildSectionAabb(Point3d topLeft, Point3d topRight, out Aabb2d aabb)
        {
            aabb = default;

            double width = topLeft.DistanceTo(topRight);
            if (width < LENGTH_TOLERANCE)
            {
                return false;
            }

            double minX = Math.Min(topLeft.X, topRight.X);
            double maxX = Math.Max(topLeft.X, topRight.X);
            double maxY = Math.Max(topLeft.Y, topRight.Y);
            double minY = maxY - width;

            aabb = new Aabb2d(minX, minY, maxX, maxY);
            return true;
        }

        private bool IsInsideAabb(Aabb2d aabb, Point3d point)
        {
            const double epsilon = 1e-6;
            return point.X >= aabb.MinX - epsilon &&
                   point.X <= aabb.MaxX + epsilon &&
                   point.Y >= aabb.MinY - epsilon &&
                   point.Y <= aabb.MaxY + epsilon;
        }

        private readonly struct SectionKey
        {
            public SectionKey(string sec, string twp, string rge, string mer)
            {
                Section = sec;
                Township = twp;
                Range = rge;
                Meridian = mer;
            }

            public string Section { get; }
            public string Township { get; }
            public string Range { get; }
            public string Meridian { get; }

            public override string ToString()
            {
                return $"SEC {Section}, TWP {Township}, RGE {Range}, MER {Meridian}";
            }
        }

        private readonly struct Aabb2d
        {
            public Aabb2d(double minX, double minY, double maxX, double maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public double MinX { get; }
            public double MinY { get; }
            public double MaxX { get; }
            public double MaxY { get; }
        }
    }
}
