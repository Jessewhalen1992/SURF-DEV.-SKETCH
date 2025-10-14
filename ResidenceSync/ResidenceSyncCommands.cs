using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.Constants;
using Autodesk.Gis.Map.ObjectData;
using Autodesk.Gis.Map.Utilities;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using DbOpenMode = Autodesk.AutoCAD.DatabaseServices.OpenMode;
using OdDataType = Autodesk.Gis.Map.Constants.DataType;
using OdOpenMode = Autodesk.Gis.Map.Constants.OpenMode;
using OdTable = Autodesk.Gis.Map.ObjectData.Table;


namespace ResidenceSync
{
    public class ResidenceSyncCommands
    {
        private const string MASTER_POINTS_PATH = @"C:\_CG_SHARED\Master_Residences.dwg";
        private const string MASTER_SECTIONS_PATH = @"C:\_CG_SHARED\Master_Sections.dwg";
        private const string PREFERRED_OD_TABLE = "SECTIONS";
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

            if (!PromptSectionPolyline(ed, out ObjectId sectionId))
            {
                return;
            }

            if (!TryGetSectionCorners(doc.Database, sectionId, out Point3d localTopLeft, out Point3d localTopRight))
            {
                ed.WriteMessage("\nPUSHRES: Failed to derive true section corners.");
                return;
            }

            if (!TryFindMasterSectionByOd(sectionKey, out Point3d masterTopLeft, out Point3d masterTopRight, out _))
            {
                ed.WriteMessage($"\nPUSHRES: Failed to locate {sectionKey} in master sections.");
                return;
            }

            if (!TryBuildSimilarity(masterTopLeft, masterTopRight, localTopLeft, localTopRight, out _, out Matrix3d sketchToMaster))
            {
                ed.WriteMessage("\nPUSHRES: Invalid similarity transform (check section geometry).");
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

                    BlockReference blockRef = tr.GetObject(sel.ObjectId, DbOpenMode.ForRead, false) as BlockReference;
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

            if (!PromptSectionPolyline(ed, out ObjectId sectionId))
            {
                return;
            }

            if (!TryGetSectionCorners(doc.Database, sectionId, out Point3d localTopLeft, out Point3d localTopRight))
            {
                ed.WriteMessage("\nPULLRES: Failed to derive true section corners.");
                return;
            }

            if (!TryFindMasterSectionByOd(sectionKey, out Point3d masterTopLeft, out Point3d masterTopRight, out Aabb2d masterAabb))
            {
                ed.WriteMessage($"\nPULLRES: Failed to locate {sectionKey} in master sections.");
                return;
            }

            if (!TryBuildSimilarity(masterTopLeft, masterTopRight, localTopLeft, localTopRight, out Matrix3d masterToSketch, out _))
            {
                ed.WriteMessage("\nPULLRES: Invalid similarity transform (check section geometry).");
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
                if (IsInsideAabb(masterAabb, pt))
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

                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, DbOpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

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

        [CommandMethod("ResidenceSync", "RESINDEX", CommandFlags.Modal)]
        public void BuildSectionIndex()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument?.Editor;
            string indexPath = Path.ChangeExtension(MASTER_SECTIONS_PATH, ".index.csv");
            if (!File.Exists(MASTER_SECTIONS_PATH))
            {
                ed?.WriteMessage($"\nRESINDEX: Master sections drawing not found: {MASTER_SECTIONS_PATH}");
                return;
            }

            DocumentCollection docs = AcadApp.DocumentManager;
            Document master = GetOpenDocumentByPath(docs, MASTER_SECTIONS_PATH);
            bool openedHere = false;
            if (master == null)
            {
                master = docs.Open(MASTER_SECTIONS_PATH, false);
                openedHere = true;
            }

            try
            {
                using (master.LockDocument())
                {
                    var project = HostMapApplicationServices.Application?.Projects?.GetProject(master);
                    if (project == null)
                    {
                        ed?.WriteMessage("\nRESINDEX: Map 3D project unavailable for master sections.");
                        return;
                    }

                    Tables tables = project.ODTables;
                    List<string> order = BuildOdTableSearchOrder(tables);

                    List<string> lines = new List<string>
            {
                "SEC,TWP,RGE,MER,TLX,TLY,TRX,TRY,MINX,MINY,MAXX,MAXY"
            };
                    HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    CultureInfo ic = CultureInfo.InvariantCulture;

                    using (Transaction tr = master.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(master.Database.BlockTableId, DbOpenMode.ForRead);
                        BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                        foreach (ObjectId id in ms)
                        {
                            Entity ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                            if (!IsPolylineEntity(ent)) continue;

                            bool captured = false;

                            foreach (string tn in order)
                            {
                                if (captured) break;
                                if (string.IsNullOrWhiteSpace(tn)) continue;

                                try
                                {
                                    using (OdTable table = tables[tn])
                                    {
                                        if (table == null) continue;

                                        FieldDefinitions defs = table.FieldDefinitions;
                                        using (Records recs = table.GetObjectTableRecords(0, ent.ObjectId, OdOpenMode.OpenForRead, true))
                                        {
                                            if (recs == null || recs.Count == 0) continue;

                                            foreach (Record rec in recs)
                                            {
                                                string sec = ReadOd(defs, rec, new[] { "SEC", "SECTION" }, MapValueToString);
                                                string twp = ReadOd(defs, rec, new[] { "TWP", "TOWNSHIP" }, MapValueToString);
                                                string rge = ReadOd(defs, rec, new[] { "RGE", "RANGE" }, MapValueToString);
                                                string mer = ReadOd(defs, rec, new[] { "MER", "MERIDIAN", "M" }, MapValueToString);
                                                if (sec == null || twp == null || rge == null || mer == null) continue;

                                                string key = $"{NormStr(sec)}|{NormStr(twp)}|{NormStr(rge)}|{NormStr(mer)}";
                                                if (!seen.Add(key))
                                                {
                                                    captured = true;
                                                    break;
                                                }

                                                Point3d tl, trPt;
                                                if (!TryGetSectionCorners(master.Database, ent.ObjectId, out tl, out trPt)) continue;
                                                Aabb2d aabb;
                                                if (!BuildSectionAabb(tl, trPt, out aabb)) continue;

                                                lines.Add(string.Join(",",
                                                    NormStr(sec),
                                                    NormStr(twp),
                                                    NormStr(rge),
                                                    NormStr(mer),
                                                    tl.X.ToString("0.########", ic),
                                                    tl.Y.ToString("0.########", ic),
                                                    trPt.X.ToString("0.########", ic),
                                                    trPt.Y.ToString("0.########", ic),
                                                    aabb.MinX.ToString("0.########", ic),
                                                    aabb.MinY.ToString("0.########", ic),
                                                    aabb.MaxX.ToString("0.########", ic),
                                                    aabb.MaxY.ToString("0.########", ic)));
                                                captured = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch { continue; }
                            }
                        }

                        tr.Commit();
                    }

                    string directory = Path.GetDirectoryName(indexPath);
                    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                    string tmp = indexPath + ".tmp";
                    File.WriteAllLines(tmp, lines);
                    if (File.Exists(indexPath)) File.Delete(indexPath);
                    File.Move(tmp, indexPath);

                    ed?.WriteMessage($"\nRESINDEX: Wrote {lines.Count - 1} rows → {indexPath}");
                }
            }
            catch
            {
                ed?.WriteMessage("\nRESINDEX: Failed to build index.");
            }
            finally
            {
                if (openedHere)
                {
                    try { master.CloseAndDiscard(); } catch { /* ignore */ }
                }
            }
        }

        private bool TryFindMasterSectionByOd(SectionKey sectionKey, out Point3d masterTopLeft, out Point3d masterTopRight, out Aabb2d masterAabb)
        {
            if (TryFindSectionFromIndex(sectionKey, out masterTopLeft, out masterTopRight, out masterAabb))
            {
                return true;
            }

            masterTopLeft = Point3d.Origin;
            masterTopRight = Point3d.Origin;
            masterAabb = default;

            if (!File.Exists(MASTER_SECTIONS_PATH))
            {
                return false;
            }

            DocumentCollection docs = AcadApp.DocumentManager;
            Document master = GetOpenDocumentByPath(docs, MASTER_SECTIONS_PATH);
            bool openedHere = false;
            if (master == null)
            {
                master = docs.Open(MASTER_SECTIONS_PATH, false);
                openedHere = true;
            }

            try
            {
                using (master.LockDocument())
                {
                    var mapApp = HostMapApplicationServices.Application;
                    var project = mapApp?.Projects?.GetProject(master);
                    if (project == null)
                    {
                        return false;
                    }

                    Tables tables = project.ODTables;
                    List<string> names = BuildOdTableSearchOrder(tables);
                    if (names == null || names.Count == 0)
                    {
                        return false;
                    }

                    ObjectId id = FindSectionPolylineByOd(master.Database, tables, names, sectionKey);
                    if (id.IsNull)
                    {
                        return false;
                    }

                    if (!TryGetSectionCorners(master.Database, id, out masterTopLeft, out masterTopRight))
                    {
                        return false;
                    }

                    if (!BuildSectionAabb(masterTopLeft, masterTopRight, out masterAabb))
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (openedHere)
                {
                    try
                    {
                        master.CloseAndDiscard();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        private bool TryFindSectionFromIndex(SectionKey key, out Point3d tl, out Point3d tr, out Aabb2d aabb)
        {
            tl = Point3d.Origin;
            tr = Point3d.Origin;
            aabb = default;

            string indexPath = Path.ChangeExtension(MASTER_SECTIONS_PATH, ".index.csv");
            if (!File.Exists(indexPath)) return false;

            string kSec = NormStr(key.Section);
            string kTwp = NormStr(key.Township);
            string kRge = NormStr(key.Range);
            string kMer = NormStr(key.Meridian);
            CultureInfo ic = CultureInfo.InvariantCulture;

            foreach (string line in File.ReadLines(indexPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("SEC,", StringComparison.OrdinalIgnoreCase)) continue;

                string[] parts = line.Split(',');
                if (parts.Length < 12) continue;

                if (!EqNorm(parts[0], kSec) || !EqNorm(parts[1], kTwp) || !EqNorm(parts[2], kRge) || !EqNorm(parts[3], kMer))
                    continue;

                double tlx = double.Parse(parts[4], ic);
                double tly = double.Parse(parts[5], ic);
                double trx = double.Parse(parts[6], ic);
                double tryVal = double.Parse(parts[7], ic);
                double minx = double.Parse(parts[8], ic);
                double miny = double.Parse(parts[9], ic);
                double maxx = double.Parse(parts[10], ic);
                double maxy = double.Parse(parts[11], ic);

                tl = new Point3d(tlx, tly, 0);
                tr = new Point3d(trx, tryVal, 0);
                aabb = new Aabb2d(minx, miny, maxx, maxy);
                return true;
            }

            return false;
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

        private static Document GetOpenDocumentByPath(DocumentCollection docCollection, string fullPath)
        {
            if (docCollection == null || string.IsNullOrWhiteSpace(fullPath))
            {
                return null;
            }

            string normalizedTarget = NormalizePath(fullPath);
            foreach (Document openDoc in docCollection)
            {
                if (openDoc == null)
                {
                    continue;
                }

                string docPath = NormalizePath(openDoc.Name);
                if (!string.IsNullOrEmpty(docPath) && string.Equals(docPath, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                {
                    return openDoc;
                }
            }

            return null;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }

        private List<string> BuildOdTableSearchOrder(Tables tables)
        {
            List<string> names = new List<string>();
            if (tables == null)
            {
                return names;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(PREFERRED_OD_TABLE) && tables.IsTableDefined(PREFERRED_OD_TABLE))
            {
                names.Add(PREFERRED_OD_TABLE);
                seen.Add(PREFERRED_OD_TABLE);
            }

            foreach (string name in tables.GetTableNames())
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (seen.Add(name))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private ObjectId FindSectionPolylineByOd(Database database, Tables tables, IEnumerable<string> tableNames, SectionKey sectionKey)
        {
            if (database == null || tables == null || tableNames == null)
            {
                return ObjectId.Null;
            }

            ObjectId matchedId = ObjectId.Null;

            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(database.BlockTableId, DbOpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                foreach (ObjectId entId in modelSpace)
                {
                    Entity entity = tr.GetObject(entId, DbOpenMode.ForRead) as Entity;
                    if (!IsPolylineEntity(entity))
                    {
                        continue;
                    }

                    foreach (string tableName in tableNames)
                    {
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            continue;
                        }

                        try
                        {
                            using (OdTable table = tables[tableName])
                            {
                                if (table == null)
                                {
                                    continue;
                                }

                                FieldDefinitions fieldDefs = table.FieldDefinitions;
                                using (Records records = table.GetObjectTableRecords(0, entity.ObjectId, OdOpenMode.OpenForRead, true))
                                {
                                    if (records == null)
                                    {
                                        continue;
                                    }

                                    foreach (Record record in records)
                                    {
                                        if (RecordMatchesSection(sectionKey, record, fieldDefs))
                                        {
                                            matchedId = entity.ObjectId;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        if (!matchedId.IsNull)
                        {
                            break;
                        }
                    }

                    if (!matchedId.IsNull)
                    {
                        break;
                    }
                }

                tr.Commit();
            }

            return matchedId;
        }

        private static bool IsPolylineEntity(Entity entity)
        {
            return entity is Polyline || entity is Polyline2d || entity is Polyline3d;
        }

        private bool RecordMatchesSection(SectionKey target, Record record, FieldDefinitions defs)
        {
            if (record == null || defs == null) return false;

            string sec = ReadOd(defs, record, new[] { "SEC", "SECTION" }, MapValueToString);
            string twp = ReadOd(defs, record, new[] { "TWP", "TOWNSHIP" }, MapValueToString);
            string rge = ReadOd(defs, record, new[] { "RGE", "RANGE" }, MapValueToString);
            string mer = ReadOd(defs, record, new[] { "MER", "MERIDIAN", "M" }, MapValueToString);

            if (sec == null || twp == null || rge == null || mer == null) return false;

            return EqNorm(sec, target.Section)
                && EqNorm(twp, target.Township)
                && EqNorm(rge, target.Range)
                && EqNorm(mer, target.Meridian);
        }

        private string MapValueToString(MapValue mv)
        {
            if (mv == null)
            {
                return null;
            }

            switch (mv.Type)
            {
                case OdDataType.Character:
                    return mv.StrValue;
                case OdDataType.Integer:
                    return mv.Int32Value.ToString(CultureInfo.InvariantCulture);
                case OdDataType.Real:
                    return mv.DoubleValue.ToString("0.####", CultureInfo.InvariantCulture);
                // case OdDataType.SmallInt:
                //     return mv.Int16Value.ToString(CultureInfo.InvariantCulture);
                // case OdDataType.BigInt:
                //     return mv.Int64Value.ToString(CultureInfo.InvariantCulture);
                default:
                    return null;
            }
        }
        // ---- Class-level helpers (C# 7.3 compatible) ----
        private static string NormStr(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            string trimmed = s.Trim();
            int n;
            if (int.TryParse(trimmed, out n)) return n.ToString(CultureInfo.InvariantCulture);
            string noZeros = trimmed.TrimStart('0');
            return noZeros.Length > 0 ? noZeros : "0";
        }

        private static bool EqNorm(string a, string b)
        {
            // numeric-robust equality: "064" == "64"
            int ai, bi;
            if (int.TryParse(a?.Trim(), out ai) && int.TryParse(b?.Trim(), out bi)) return ai == bi;

            string aa = NormStr(a);
            string bb = NormStr(b);
            if (int.TryParse(aa, out ai) && int.TryParse(bb, out bi)) return ai == bi;

            return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadOd(FieldDefinitions defs, Record rec, string[] aliases, Func<MapValue, string> toString)
        {
            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                if (def == null) continue;
                if (!aliases.Any(a => a.Equals(def.Name, StringComparison.OrdinalIgnoreCase))) continue;

                MapValue mv;
                try { mv = rec[i]; } catch { continue; }

                string s = toString(mv);
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
            return null;
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
                Entity ent = tr.GetObject(polylineId, DbOpenMode.ForRead) as Entity;
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
                        Vertex2d vert = tr.GetObject(vertId, DbOpenMode.ForRead) as Vertex2d;
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
                        PolylineVertex3d vert = tr.GetObject(vertId, DbOpenMode.ForRead) as PolylineVertex3d;
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
        [CommandMethod("ResidenceSync", "RESCHECK", CommandFlags.Modal)]
        public void CheckSectionIndex()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument?.Editor;
            if (!PromptSectionKey(ed, out SectionKey key)) return;

            Point3d tl, tr; Aabb2d box;
            if (TryFindSectionFromIndex(key, out tl, out tr, out box))
            {
                ed?.WriteMessage($"\nRESCHECK: FOUND in CSV → TL=({tl.X:0.###},{tl.Y:0.###}) TR=({tr.X:0.###},{tr.Y:0.###}) AABB=[{box.MinX:0.###},{box.MinY:0.###}]→[{box.MaxX:0.###},{box.MaxY:0.###}]");
            }
            else
            {
                ed?.WriteMessage("\nRESCHECK: NOT in CSV. Run RESINDEX or verify OD field names (SEC/TWP/RGE/MER or M).");
            }
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

            string directory = Path.GetDirectoryName(MASTER_POINTS_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(MASTER_POINTS_PATH))
            {
                using (Database newDb = new Database(true, true))
                {
                    newDb.SaveAs(MASTER_POINTS_PATH, DwgVersion.Current);
                }
            }

            int appended = 0;
            using (Database masterDb = new Database(false, true))
            {
                masterDb.ReadDwgFile(MASTER_POINTS_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                masterDb.CloseInput(true);

                using (Transaction tr = masterDb.TransactionManager.StartTransaction())
                {
                    EnsureLayer(masterDb, RESIDENCE_LAYER, tr);
                    // Resolve the layer ObjectId once, then reuse
                    LayerTable lt = (LayerTable)tr.GetObject(masterDb.LayerTableId, DbOpenMode.ForRead);
                    ObjectId resLayerId = lt[RESIDENCE_LAYER];

                    BlockTable bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, DbOpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

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

                masterDb.SaveAs(MASTER_POINTS_PATH, DwgVersion.Current);
            }

            return appended;
        }

        private List<Point3d> ReadPointsFromMaster(out bool exists)
        {
            exists = File.Exists(MASTER_POINTS_PATH);
            if (!exists)
            {
                return new List<Point3d>();
            }

            List<Point3d> points = new List<Point3d>();
            using (Database masterDb = new Database(false, true))
            {
                masterDb.ReadDwgFile(MASTER_POINTS_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                masterDb.CloseInput(true);

                using (Transaction tr = masterDb.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, DbOpenMode.ForRead);
                    BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                    foreach (ObjectId id in modelSpace)
                    {
                        Entity ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
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
            LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, DbOpenMode.ForRead);
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
                Section = (sec ?? string.Empty).Trim();
                Township = (twp ?? string.Empty).Trim();
                Range = (rge ?? string.Empty).Trim();
                Meridian = (mer ?? string.Empty).Trim();
            }

            public string Section { get; }
            public string Township { get; }
            public string Range { get; }
            public string Meridian { get; }

            public string SectionNormalized => Normalize(Section);
            public string TownshipNormalized => Normalize(Township);
            public string RangeNormalized => Normalize(Range);
            public string MeridianNormalized => Normalize(Meridian);

            public bool Equals(string section, string township, string range, string meridian)
            {
                return NumbersEqual(Section, section)
                    && NumbersEqual(Township, township)
                    && NumbersEqual(Range, range)
                    && NumbersEqual(Meridian, meridian);
            }

            public override string ToString()
            {
                return $"SEC {Section}, TWP {Township}, RGE {Range}, MER {Meridian}";
            }

            private static bool NumbersEqual(string a, string b)
            {
                if (int.TryParse(a?.Trim(), out int ai) && int.TryParse(b?.Trim(), out int bi))
                {
                    return ai == bi;
                }

                string aa = Normalize(a);
                string bb = Normalize(b);
                if (int.TryParse(aa, out ai) && int.TryParse(bb, out bi))
                {
                    return ai == bi;
                }

                return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            private static string Normalize(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return string.Empty;
                }

                string trimmed = s.Trim();
                if (int.TryParse(trimmed, out int n))
                {
                    return n.ToString(CultureInfo.InvariantCulture);
                }

                string noZeros = trimmed.TrimStart('0');
                return noZeros.Length > 0 ? noZeros : "0";
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
