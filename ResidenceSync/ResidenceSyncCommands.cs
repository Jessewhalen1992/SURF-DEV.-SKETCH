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

                    // If multiple entities carry the same OD, keep the one with **largest vertex AABB area**
                    var bestByKey = new Dictionary<string, (double area, double tlx, double tly, double trx, double @try, Aabb2d aabb)>(StringComparer.OrdinalIgnoreCase);

                    using (Transaction tr = master.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(master.Database.BlockTableId, DbOpenMode.ForRead);
                        BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                        int scanned = 0;
                        foreach (ObjectId id in ms)
                        {
                            Entity ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                            if (!IsPolylineEntity(ent)) continue;

                            // Get robust TL/TR and AABB from this entity's **vertices only**
                            double tlx, tly, trx, @try;
                            Aabb2d aabb;
                            if (!TryGetCornersFromVerticesStrict(master.Database, id, tr, out tlx, out tly, out trx, out @try, out aabb))
                                continue;

                            foreach (string tn in order)
                            {
                                if (string.IsNullOrWhiteSpace(tn)) continue;

                                using (OdTable t = tables[tn])
                                {
                                    if (t == null) continue;
                                    FieldDefinitions defs = t.FieldDefinitions;

                                    using (Records recs = t.GetObjectTableRecords(0, ent.ObjectId, OdOpenMode.OpenForRead, true))
                                    {
                                        if (recs == null || recs.Count == 0) continue;

                                        foreach (Record rec in recs)
                                        {
                                            string sec = ReadOd(defs, rec, new[] { "SEC", "SECTION" }, MapValueToString);
                                            string twp = ReadOd(defs, rec, new[] { "TWP", "TOWNSHIP" }, MapValueToString);
                                            string rge = ReadOd(defs, rec, new[] { "RGE", "RANGE" }, MapValueToString);
                                            string mer = ReadOd(defs, rec, new[] { "MER", "MERIDIAN", "M" }, MapValueToString);
                                            if (string.IsNullOrWhiteSpace(sec) ||
                                                string.IsNullOrWhiteSpace(twp) ||
                                                string.IsNullOrWhiteSpace(rge) ||
                                                string.IsNullOrWhiteSpace(mer))
                                                continue;

                                            string kSec = NormStr(sec), kTwp = NormStr(twp), kRge = NormStr(rge), kMer = NormStr(mer);
                                            string key = $"{kSec}|{kTwp}|{kRge}|{kMer}";

                                            double area = (aabb.MaxX - aabb.MinX) * (aabb.MaxY - aabb.MinY);
                                            if (!bestByKey.TryGetValue(key, out var cur) || area > cur.area)
                                            {
                                                bestByKey[key] = (area, tlx, tly, trx, @try, aabb);
                                            }
                                        }
                                    }
                                }
                            }

                            scanned++;
                            if ((scanned % 10000) == 0) ed?.WriteMessage($"\nRESINDEX: scanned {scanned} entities...");
                        }

                        tr.Commit();
                    }

                    // Write CSV
                    var ic = CultureInfo.InvariantCulture;
                    var lines = new List<string>(bestByKey.Count + 8)
            {
                "SEC,TWP,RGE,MER,TLX,TLY,TRX,TRY,MINX,MINY,MAXX,MAXY"
            };

                    foreach (var kv in bestByKey)
                    {
                        string key = kv.Key;
                        var v = kv.Value;
                        var parts = key.Split('|');
                        string sec = parts[0], twp = parts[1], rge = parts[2], mer = parts[3];

                        lines.Add(string.Join(",",
                            sec, twp, rge, mer,
                            v.tlx.ToString("0.########", ic),
                            v.tly.ToString("0.########", ic),
                            v.trx.ToString("0.########", ic),
                            v.@try.ToString("0.########", ic),
                            v.aabb.MinX.ToString("0.########", ic),
                            v.aabb.MinY.ToString("0.########", ic),
                            v.aabb.MaxX.ToString("0.########", ic),
                            v.aabb.MaxY.ToString("0.########", ic)
                        ));
                    }

                    string dir = Path.GetDirectoryName(indexPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    string tmp = indexPath + ".tmp";
                    File.WriteAllLines(tmp, lines);
                    if (File.Exists(indexPath)) File.Delete(indexPath);
                    File.Move(tmp, indexPath);

                    ed?.WriteMessage($"\nRESINDEX: Wrote {lines.Count - 1} rows → {indexPath}");
                }
            }
            catch (System.Exception ex)
            {
                ed?.WriteMessage($"\nRESINDEX: Failed: {ex.Message}");
            }
            finally
            {
                if (openedHere)
                {
                    try { master.CloseAndDiscard(); } catch { /* ignore */ }
                }
            }
        }
        // Derive TL/TR and AABB strictly from this polyline's **own vertices**.
        // Robust to tiny tilt/vertex jitter. Never uses neighboring geometry.
        // Derive TL/TR strictly from this polyline’s vertices.
        // 1) Build AABB from vertices.
        // 2) Take a "top band" = points with Y within yTol of maxY.
        // 3) TL = minX within top band; TR = maxX within top band.
        private bool TryGetCornersFromVerticesStrict(Database db, ObjectId id, Transaction tr,
            out double tlx, out double tly, out double trx, out double @try, out Aabb2d aabb)
        {
            tlx = tly = trx = @try = 0;
            aabb = default;

            var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
            if (ent == null) return false;

            var pts = new List<Point3d>(16);

            if (ent is Polyline pl)
            {
                int n = pl.NumberOfVertices;
                for (int i = 0; i < n; i++) pts.Add(pl.GetPoint3dAt(i));
            }
            else if (ent is Polyline2d pl2)
            {
                foreach (ObjectId vId in pl2)
                {
                    var v = (Vertex2d)tr.GetObject(vId, DbOpenMode.ForRead);
                    pts.Add(v.Position);
                }
            }
            else if (ent is Polyline3d pl3)
            {
                foreach (ObjectId vId in pl3)
                {
                    var v = (PolylineVertex3d)tr.GetObject(vId, DbOpenMode.ForRead);
                    pts.Add(new Point3d(v.Position.X, v.Position.Y, 0));
                }
            }
            else return false;

            if (pts.Count < 4) return false;

            // --- Compute 2D convex hull for robustness ---
            // helps remove interior vertices and isolate outline.
            var hull = ConvexHull2D(pts);
            if (hull.Count < 3) hull = pts;

            double minX = hull.Min(p => p.X);
            double maxX = hull.Max(p => p.X);
            double minY = hull.Min(p => p.Y);
            double maxY = hull.Max(p => p.Y);
            aabb = new Aabb2d(minX, minY, maxX, maxY);

            // --- Find the longest horizontal (or near-horizontal) edge with highest average Y ---
            // --- Find the longest horizontal (or near-horizontal) edge with highest average Y ---
            double bestY = double.MinValue;
            Point3d bestA = Point3d.Origin, bestB = Point3d.Origin;

            const double tolAngle = 5 * Math.PI / 180.0; // within 5 degrees of horizontal

            for (int i = 0; i < hull.Count; i++)
            {
                Point3d p1 = hull[i];
                Point3d p2 = hull[(i + 1) % hull.Count];
                Vector3d v = p2 - p1;

                // Determine how close this segment is to horizontal (using Z-axis as normal)
                double ang = v.GetAngleTo(Vector3d.XAxis, Vector3d.ZAxis);
                if (ang > tolAngle && Math.Abs(ang - Math.PI) > tolAngle)
                    continue; // not roughly horizontal

                double avgY = (p1.Y + p2.Y) * 0.5;
                if (avgY > bestY)
                {
                    bestY = avgY;
                    bestA = p1;
                    bestB = p2;
                }
            }

            if (bestY == double.MinValue)
            {
                // fallback: axis-aligned
                tlx = minX; tly = maxY;
                trx = maxX; @try = maxY;
                return true;
            }

            // Order left->right based on X
            if (bestA.X <= bestB.X)
            {
                tlx = bestA.X; tly = bestA.Y;
                trx = bestB.X; @try = bestB.Y;
            }
            else
            {
                tlx = bestB.X; tly = bestB.Y;
                trx = bestA.X; @try = bestA.Y;
            }

            return true;
        }

        // --- Simple convex hull helper (Graham scan) ---
        private static List<Point3d> ConvexHull2D(List<Point3d> pts)
        {
            if (pts.Count <= 3) return new List<Point3d>(pts);

            var sorted = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            List<Point3d> lower = new List<Point3d>();
            foreach (var p in sorted)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Point3d> upper = new List<Point3d>();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                var p = sorted[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        private static double Cross(Point3d o, Point3d a, Point3d b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        private bool TryGetEntityAabb(Database db, ObjectId id, Transaction tr, out Aabb2d aabb)
        {
            aabb = default;
            try
            {
                var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                if (ent == null) return false;

                try
                {
                    Extents3d ext = ent.GeometricExtents; // includes arcs/bulges, rotation-safe
                    aabb = new Aabb2d(ext.MinPoint.X, ext.MinPoint.Y, ext.MaxPoint.X, ext.MaxPoint.Y);
                    return true;
                }
                catch
                {
                    // Fallback: vertex sweep
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;

                    if (ent is Polyline pl)
                    {
                        int n = pl.NumberOfVertices;
                        for (int i = 0; i < n; i++)
                        {
                            Point3d p = pl.GetPoint3dAt(i);
                            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
                            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
                        }
                    }
                    else if (ent is Polyline2d pl2)
                    {
                        foreach (ObjectId vId in pl2)
                        {
                            var v = (Vertex2d)tr.GetObject(vId, DbOpenMode.ForRead);
                            Point3d p = v.Position;
                            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
                            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
                        }
                    }
                    else if (ent is Polyline3d pl3)
                    {
                        foreach (ObjectId vId in pl3)
                        {
                            var v = (PolylineVertex3d)tr.GetObject(vId, DbOpenMode.ForRead);
                            Point3d p = new Point3d(v.Position.X, v.Position.Y, 0);
                            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
                            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (minX == double.MaxValue) return false;
                    aabb = new Aabb2d(minX, minY, maxX, maxY);
                    return true;
                }
            }
            catch { return false; }
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
            if (!File.Exists(indexPath))
                return false;

            CultureInfo ic = CultureInfo.InvariantCulture;

            // Normalize the key parts once
            string kSec = NormalizeKeyNumber(key.Section, ic);
            string kTwp = NormalizeKeyNumber(key.Township, ic);
            string kRge = NormalizeKeyNumber(key.Range, ic);
            string kMer = NormalizeKeyNumber(key.Meridian, ic);

            try
            {
                // Open with sharing + tiny retry for transient locks
                System.IO.StreamReader reader = null;
                for (int attempt = 0; attempt < 6; attempt++)
                {
                    try
                    {
                        var fs = new FileStream(indexPath, FileMode.Open, FileAccess.Read,
                                                FileShare.ReadWrite | FileShare.Delete);
                        reader = new StreamReader(fs);
                        break;
                    }
                    catch (IOException)
                    {
                        // brief backoff (total ~375 ms worst case)
                        System.Threading.Thread.Sleep(75);
                    }
                }
                if (reader == null) throw new IOException("Unable to open index CSV (locked).");

                using (reader)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("SEC,", StringComparison.OrdinalIgnoreCase)) continue;

                        string[] parts = line.Split(',');
                        if (parts.Length < 12) continue;

                        // Match on normalized numbers (handles leading zeros etc.)
                        if (!NumbersEqual(parts[0], kSec, ic) ||
                            !NumbersEqual(parts[1], kTwp, ic) ||
                            !NumbersEqual(parts[2], kRge, ic) ||
                            !NumbersEqual(parts[3], kMer, ic))
                        {
                            continue;
                        }

                        // Parse doubles
                        if (!double.TryParse(parts[4], NumberStyles.Float, ic, out double tlx)) continue;
                        if (!double.TryParse(parts[5], NumberStyles.Float, ic, out double tly)) continue;
                        if (!double.TryParse(parts[6], NumberStyles.Float, ic, out double trxVal)) continue;
                        if (!double.TryParse(parts[7], NumberStyles.Float, ic, out double tryVal)) continue;
                        if (!double.TryParse(parts[8], NumberStyles.Float, ic, out double minx)) continue;
                        if (!double.TryParse(parts[9], NumberStyles.Float, ic, out double miny)) continue;
                        if (!double.TryParse(parts[10], NumberStyles.Float, ic, out double maxx)) continue;
                        if (!double.TryParse(parts[11], NumberStyles.Float, ic, out double maxy)) continue;

                        tl = new Point3d(tlx, tly, 0);
                        tr = new Point3d(trxVal, tryVal, 0);
                        aabb = new Aabb2d(minx, miny, maxx, maxy);
                        return true;
                    }
                }
            }
            catch (IOException)
            {
                // Friendly note if something still has an exclusive lock
                var ed = AcadApp.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage("\nNote: Could not read Master_Sections.index.csv (locked by another process). Close it and try again.");
                return false;
            }
            catch
            {
                return false;
            }

            return false;

            // ---- local helpers (non-static to stay C# 7.3 compatible) ----
            string NormalizeKeyNumber(string s, IFormatProvider provider)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                s = s.Trim();
                if (int.TryParse(s, NumberStyles.Integer, provider, out int n))
                    return n.ToString(provider);

                string noZeros = s.TrimStart('0');
                return noZeros.Length > 0 ? noZeros : "0";
            }

            bool NumbersEqual(string a, string b, IFormatProvider provider)
            {
                // Compare using normalized forms
                string na = NormalizeKeyNumber(a, provider);
                string nb = NormalizeKeyNumber(b, provider);
                return string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
            }
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
        // SURFDEV – build 3×3 sections from CSV and rings, placed at a user insertion point.
        // Prompts: SD centre (rings) -> scale -> SEC/TWP/RGE/MER -> TL/TR (orientation) -> insertion point (stamp placement)
        // SURFDEV – build 3×3 sections from CSV and rings, placed at a user insertion point.
        // Prompts: SD centre (rings) -> scale -> SEC/TWP/RGE/MER -> TL/TR (orientation) -> insertion point (stamp placement)
        // SURFDEV — 3×3 Surface Development from CSV
        // - One canonical orientation from the center section (TL->TR in master).
        // - Spacing EW/NS taken from neighbors in CSV (captures road-allowance gaps).
        // - Scale from map scale (50k/25k/20k), NOT from the pick length.
        // - Rings centered at the SD center pick, sized by map scale.
        // - Insert so the middle-section center = the user’s insertion point.
        // SURFDEV — Axis-aligned 3×3 grid from CSV (no rotation from TL/TR).
        // Steps:
        // 1) Pick SD centre (for rings only)
        // 2) Pick scale (50k/25k/20k) → sets section/ring/text sizes
        // 3–6) Enter SEC/TWP/RGE/MER (center section)
        // 7–8) Pick TL/TR of a real section (ONLY to measure rings offset relative to that section)
        // 9) Pick insertion point (centre of middle section)
        // Result: draw 3×3 sections from CSV in a single canonical basis, scaled by map scale;
        //         place rings at insertion-centre + normalized offset from the TL/TR pick.
        // SURFDEV — Axis-aligned 3×3 grid from CSV (no rotation from TL/TR).
        // TL/TR pick is used ONLY to compute the rings’ relative offset; grid scale = map scale.
        // SURFDEV — Draw the 3×3 grid strictly from CSV positions (per-section AABB).
        // - No inferred stepping; each section uses its own CSV TL/TR/AABB.
        // - Axis-aligned stamp: scale only from map scale (50k/25k/20k).
        // - TL/TR pick is used ONLY to compute rings' relative offset from the middle-section center.
        // - Insertion point places the middle-section center.
        // - Rings: 0.5 / 1.5 / 2.0 km radii.
        [CommandMethod("ResidenceSync", "SURFDEV", CommandFlags.Modal)]
        public void DrawSurfaceDevelopment3x3()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            // 1) SD centre (for rings only – we’ll convert this into a normalized offset)
            var pC = ed.GetPoint("\nPick Centre of Surface Development (rings only): ");
            if (pC.Status != PromptStatus.OK) return;
            Point3d sdCenterPicked = pC.Value;

            // 2) Map scale → drawing units per km / metre
            var pko = new PromptKeywordOptions("\nPick scale [50k/25k/20k]: ") { AllowNone = false };
            pko.Keywords.Add("50k"); pko.Keywords.Add("25k"); pko.Keywords.Add("20k");
            var kres = ed.GetKeywords(pko);
            if (kres.Status != PromptStatus.OK) return;
            string scaleKey = (kres.StringResult ?? "50k").ToLowerInvariant();

            // Per your earlier spec: 2.0 km ring = 200 @ 1:50k → 100 units per km
            double unitsPerKm = (scaleKey == "25k") ? 200.0 : (scaleKey == "20k") ? 250.0 : 100.0;
            double unitsPerMetre = unitsPerKm / 1000.0;
            double secTextHt = (scaleKey == "50k") ? 15.0 : (scaleKey == "25k") ? 30.0 : 37.5;
            double ringTextHt = (scaleKey == "50k") ? 8.0 : (scaleKey == "25k") ? 16.0 : 20.0;

            // 3–6) Center section key
            if (!PromptSectionKey(ed, out SectionKey key)) return;

            // 7–8) Pick TL/TR on THIS SKETCH (only to measure where you want the rings relative to the NORTH edge)
            if (!PromptCornerPair(ed, out Point3d tlPick, out Point3d trPick)) return;
            Vector3d eastS = (trPick - tlPick).GetNormal();
            Vector3d northS = eastS.RotateBy(Math.PI / 2.0, Vector3d.ZAxis).GetNormal();
            double wPick = trPick.DistanceTo(tlPick);
            if (wPick < 1e-6) { ed.WriteMessage("\nSURFDEV: TL/TR distance too small."); return; }

            // A normalized offset of your ring centre **from the north edge mid** (not the section centre)
            Point3d midNorthPick = tlPick + 0.5 * (trPick - tlPick);  // mid-point of northern edge (in the pick frame)
            Vector3d offPick = sdCenterPicked - midNorthPick;
            double uNorm = offPick.DotProduct(eastS) / Math.Max(1e-9, wPick); // along east (+) / west (–)
            double vNorm = offPick.DotProduct(northS) / Math.Max(1e-9, wPick); // +down is negative (since north points up)

            // 9) Insertion point (centre of middle section in SKETCH frame)
            var pIns = ed.GetPoint("\nPick insertion point (centre of middle section): ");
            if (pIns.Status != PromptStatus.OK) return;
            Point3d insertCenter = pIns.Value;

            // ---- CSV lookup for center section (26–64–3–6 example you provided) ----
            if (!TryFindSectionFromIndex(key, out Point3d TLm_c, out Point3d TRm_c, out Aabb2d BBc))
            {
                ed.WriteMessage("\nSURFDEV: Selected section not found in CSV index.");
                return;
            }

            // MASTER frame: compute the *true* north (top) edge and the section height from CSV AABB
            Vector3d eastM_c = (TRm_c - TLm_c);
            double wM_c = eastM_c.Length;
            if (wM_c < 1e-6) { ed.WriteMessage("\nSURFDEV: Degenerate north edge in CSV."); return; }
            eastM_c = eastM_c / wM_c;
            double hM_c = Math.Max(1e-6, BBc.MaxY - BBc.MinY);
            Vector3d northM_c = eastM_c.RotateBy(Math.PI / 2.0, Vector3d.ZAxis); // points “up” from north edge

            // Centre of the middle section in MASTER frame (use the AABB centre)
            Point3d centerMm = new Point3d((BBc.MinX + BBc.MaxX) * 0.5, (BBc.MinY + BBc.MaxY) * 0.5, 0);

            // MASTER → SKETCH mapping: uniform **scale only**, no rotation; preserve master orientation
            Point3d Map(Point3d pm)
            {
                Vector3d d = pm - centerMm;
                return insertCenter + new Vector3d(d.X * unitsPerMetre, d.Y * unitsPerMetre, 0);
            }

            // 6×6 serpentine numbering grid for neighbours (row 0 = north/top, col 0 = west/left)
            int[,] serp =
            {
        {31,32,33,34,35,36},
        {30,29,28,27,26,25},
        {19,20,21,22,23,24},
        {18,17,16,15,14,13},
        { 7, 8, 9,10,11,12},
        { 6, 5, 4, 3, 2, 1}
    };

            // Locate selected SEC in serp
            if (!int.TryParse(key.Section.TrimStart('0'), out int secNum))
            { ed.WriteMessage("\nSURFDEV: SEC not numeric."); return; }

            int selRow = -1, selCol = -1;
            for (int r = 0; r < 6 && selRow < 0; r++)
                for (int c = 0; c < 6; c++)
                    if (serp[r, c] == secNum) { selRow = r; selCol = c; break; }
            if (selRow < 0) { ed.WriteMessage("\nSURFDEV: SEC not in 1..36."); return; }

            SectionKey KeyAt(int row, int col)
            {
                if (row < 0 || row > 5 || col < 0 || col > 5) return default(SectionKey);
                int s = serp[row, col];
                return new SectionKey(s.ToString(), key.Township, key.Range, key.Meridian);
            }

            using (doc.LockDocument())
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Layers + colors
                EnsureLayer(doc.Database, "L-USEC", tr);
                EnsureLayer(doc.Database, "L-QSEC", tr);
                EnsureLayer(doc.Database, "AUX-BUFFER", tr);

                var lt = (LayerTable)tr.GetObject(doc.Database.LayerTableId, DbOpenMode.ForRead);
                ((LayerTableRecord)tr.GetObject(lt["L-USEC"], DbOpenMode.ForWrite)).Color =
                    Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 253);
                ((LayerTableRecord)tr.GetObject(lt["L-QSEC"], DbOpenMode.ForWrite)).Color =
                    Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 254);

                var bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, DbOpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

                // Draw each of the 9 sections using **true north edge** (TL/TR from CSV) + **CSV height**
                for (int dRow = -1; dRow <= 1; dRow++)       // -1 north, +1 south
                {
                    for (int dCol = -1; dCol <= 1; dCol++)   // -1 west, +1 east
                    {
                        SectionKey k2 = KeyAt(selRow + dRow, selCol + dCol);
                        if (string.IsNullOrEmpty(k2.Section)) continue;

                        if (!TryFindSectionFromIndex(k2, out Point3d TLm, out Point3d TRm, out Aabb2d BB))
                            continue; // missing neighbour → skip

                        Vector3d eastM = (TRm - TLm);
                        double wM = eastM.Length;
                        if (wM < 1e-6) continue;
                        eastM = eastM / wM;

                        double hM = Math.Max(1e-6, BB.MaxY - BB.MinY);         // **height from CSV**, not assumed = width
                        Vector3d northM = eastM.RotateBy(Math.PI / 2.0, Vector3d.ZAxis);

                        // Build FOUR corners from TL/TR + height **in MASTER**
                        Point3d TLs = Map(TLm);
                        Point3d TRs = Map(TRm);
                        Point3d BLs = Map(TLm - northM * hM);
                        Point3d BRs = Map(TRm - northM * hM);

                        // Outline – 4 lines (prevents bridging)
                        var l1 = new Line(TLs, TRs) { Layer = "L-USEC" };
                        var l2 = new Line(TRs, BRs) { Layer = "L-USEC" };
                        var l3 = new Line(BRs, BLs) { Layer = "L-USEC" };
                        var l4 = new Line(BLs, TLs) { Layer = "L-USEC" };
                        ms.AppendEntity(l1); tr.AddNewlyCreatedDBObject(l1, true);
                        ms.AppendEntity(l2); tr.AddNewlyCreatedDBObject(l2, true);
                        ms.AppendEntity(l3); tr.AddNewlyCreatedDBObject(l3, true);
                        ms.AppendEntity(l4); tr.AddNewlyCreatedDBObject(l4, true);

                        // Quarter lines confined to THIS section only
                        Point3d midTop = new Point3d((TLs.X + TRs.X) * 0.5, (TLs.Y + TRs.Y) * 0.5, 0);
                        Point3d midBot = new Point3d((BLs.X + BRs.X) * 0.5, (BLs.Y + BRs.Y) * 0.5, 0);
                        Point3d midLft = new Point3d((TLs.X + BLs.X) * 0.5, (TLs.Y + BLs.Y) * 0.5, 0);
                        Point3d midRgt = new Point3d((TRs.X + BRs.X) * 0.5, (TRs.Y + BRs.Y) * 0.5, 0);

                        var qv = new Line(midTop, midBot) { Layer = "L-QSEC" };
                        var qh = new Line(midLft, midRgt) { Layer = "L-QSEC" };
                        ms.AppendEntity(qv); tr.AddNewlyCreatedDBObject(qv, true);
                        ms.AppendEntity(qh); tr.AddNewlyCreatedDBObject(qh, true);

                        // Optional section number at geometric centre
                        Point3d ctr = new Point3d(
                            (TLs.X + TRs.X + BLs.X + BRs.X) / 4.0,
                            (TLs.Y + TRs.Y + BLs.Y + BRs.Y) / 4.0, 0);
                        var tx = new DBText
                        {
                            Position = ctr,
                            Height = secTextHt,
                            TextString = k2.Section.TrimStart('0'),
                            Layer = "AUX-BUFFER",
                            HorizontalMode = TextHorizontalMode.TextCenter,
                            VerticalMode = TextVerticalMode.TextVerticalMid,
                            AlignmentPoint = ctr
                        };
                        ms.AppendEntity(tx); tr.AddNewlyCreatedDBObject(tx, true);
                    }
                }

                // ----- RINGS -----
                // Place rings using the normalized offset **from the NORTH edge mid** of the centre section.
                // Compute that north-edge mid in SKETCH:
                Point3d TLs_c = Map(TLm_c);
                Point3d TRs_c = Map(TRm_c);
                Point3d midNorthSketch = new Point3d((TLs_c.X + TRs_c.X) * 0.5, (TLs_c.Y + TRs_c.Y) * 0.5, 0);
                Vector3d eastS_c = (TRs_c - TLs_c).GetNormal();
                double wS_c = TLs_c.DistanceTo(TRs_c);
                Vector3d northS_c = eastS_c.RotateBy(Math.PI / 2.0, Vector3d.ZAxis).GetNormal();

                // Rebuild ring centre from normalized offsets in SKETCH units
                Point3d ringCentreFinal = midNorthSketch + (eastS_c * (uNorm * wS_c)) + (northS_c * (vNorm * wS_c));

                double[] radiiKm = { 0.5, 1.5, 2.0 };
                foreach (double km in radiiKm)
                {
                    double r = km * unitsPerKm;
                    var c = new Circle(ringCentreFinal, Vector3d.ZAxis, r) { Layer = "AUX-BUFFER" };
                    ms.AppendEntity(c); tr.AddNewlyCreatedDBObject(c, true);

                    Point3d labPt = ringCentreFinal + new Vector3d(r + (0.1 * unitsPerKm), 0, 0);
                    var lab = new DBText
                    {
                        Position = labPt,
                        Height = ringTextHt,
                        TextString = km.ToString("0.## km"),
                        Layer = "AUX-BUFFER",
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        AlignmentPoint = labPt
                    };
                    ms.AppendEntity(lab); tr.AddNewlyCreatedDBObject(lab, true);
                }

                tr.Commit();
            }

            ed.WriteMessage("\nSURFDEV: Sections drawn from CSV north edge (TL/TR) + CSV height; rings placed by normalized offset from north edge.");
        }
        // Compute TL/TR and AABB strictly from this entity's vertices (no GeometricExtents).
        // TL/TR come from the "top band" (maxY within tolerance) to resist tiny tilt/bulge.
        private bool TryGetSectionBoxFromVertices(
            Database db, ObjectId id, Transaction tr,
            out double tlx, out double tly, out double trx, out double try_,
            out Aabb2d aabb)
        {
            tlx = tly = trx = try_ = 0.0;
            aabb = default;

            var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
            if (ent == null) return false;

            var pts = new List<Point3d>(16);

            if (ent is Polyline pl)
            {
                int n = pl.NumberOfVertices;
                for (int i = 0; i < n; i++) pts.Add(pl.GetPoint3dAt(i));
            }
            else if (ent is Polyline2d pl2)
            {
                foreach (ObjectId vId in pl2)
                {
                    var v = (Vertex2d)tr.GetObject(vId, DbOpenMode.ForRead);
                    pts.Add(v.Position);
                }
            }
            else if (ent is Polyline3d pl3)
            {
                foreach (ObjectId vId in pl3)
                {
                    var v = (PolylineVertex3d)tr.GetObject(vId, DbOpenMode.ForRead);
                    pts.Add(new Point3d(v.Position.X, v.Position.Y, 0));
                }
            }
            else return false;

            if (pts.Count == 0) return false;

            // AABB from vertices only
            double minX = pts.Min(p => p.X);
            double maxX = pts.Max(p => p.X);
            double minY = pts.Min(p => p.Y);
            double maxY = pts.Max(p => p.Y);
            aabb = new Aabb2d(minX, minY, maxX, maxY);

            // "Top band" selection: all verts within tol of top Y
            const double topTol = 0.05; // metres; bump to 0.1 if your data is noisier
            var topPts = pts.Where(p => (maxY - p.Y) <= topTol).ToList();
            if (topPts.Count == 0) topPts = pts;  // defensive fallback

            var tl = topPts.OrderBy(p => p.X).First();
            var trp = topPts.OrderByDescending(p => p.X).First();

            tlx = tl.X; tly = tl.Y;
            trx = trp.X; try_ = trp.Y;
            return true;
        }
        // Heuristic: how likely this AABB is a single DLS section (lower is better)
        private double ScoreAabbAsSection(Aabb2d aabb)
        {
            // Nominal DLS section (interior) ~ 1609.344 m each side (1 mile), but RA/corrections vary
            const double mile = 1609.344;
            double w = aabb.MaxX - aabb.MinX;
            double h = aabb.MaxY - aabb.MinY;
            if (w <= 0 || h <= 0) return double.MaxValue;

            // Quick reject: absurd sizes (too small/too large)
            if (w < 800 || h < 800 || w > 4000 || h > 4000) return 1e9;

            // Prefer square-ish (width≈height), and both close to a mile
            double squarePenalty = Math.Abs(w - h);
            double sizePenalty = Math.Abs(w - mile) + Math.Abs(h - mile);

            // Mild preference for reasonably large area (avoid tiny sliver outlines)
            double area = w * h;
            double areaPenalty = 0.0;
            if (area < 1.0e6) areaPenalty = (1.0e6 - area) * 0.001; // weaker weight

            return squarePenalty + sizePenalty * 0.5 + areaPenalty;
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
