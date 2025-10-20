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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // Tolerances (metres, WCS)
        private const double DEDUPE_TOL = 0.25;  // merge if within 25 cm
        private const double REPLACE_TOL = 3.0;   // replace if within 3 m
        private const double ERASE_TOL = 0.001;  // polygon test epsilon (ray-cast denom guard)
        private const double TRANSFORM_VALIDATION_TOL = 0.75; // scaled push must align within < 1 m

        // =========================================================================
        // RESINDEXV — Build vertex index (JSONL) from Master_Sections.dwg
        // =========================================================================
        [CommandMethod("ResidenceSync", "RESINDEXV", CommandFlags.Modal)]
        public void BuildVertexIndex()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument?.Editor;

            if (!File.Exists(MASTER_SECTIONS_PATH))
            {
                ed?.WriteMessage($"\nRESINDEXV: Master sections DWG not found: {MASTER_SECTIONS_PATH}");
                return;
            }

            string outPath = Path.Combine(Path.GetDirectoryName(MASTER_SECTIONS_PATH) ?? "",
                                          "Master_Sections.index.jsonl");

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
                        ed?.WriteMessage("\nRESINDEXV: Map 3D project unavailable.");
                        return;
                    }

                    Tables tables = project.ODTables;
                    var searchOrder = BuildOdTableSearchOrder(tables);

                    // Pick "best" outline per section key: largest AABB area; store all vertices
                    var bestByKey = new Dictionary<string, (Aabb2d aabb, ObjectId entId)>(StringComparer.OrdinalIgnoreCase);

                    using (Transaction tr = master.TransactionManager.StartTransaction())
                    {
                        var bt = (BlockTable)tr.GetObject(master.Database.BlockTableId, DbOpenMode.ForRead);
                        var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                        int scanned = 0;
                        foreach (ObjectId id in ms)
                        {
                            var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                            if (!IsPolylineEntity(ent)) continue;

                            if (!TryGetEntityAabb(master.Database, id, tr, out var aabb)) continue;

                            foreach (string tn in searchOrder)
                            {
                                if (string.IsNullOrWhiteSpace(tn)) continue;
                                using (OdTable t = tables[tn])
                                {
                                    if (t == null) continue;
                                    var defs = t.FieldDefinitions;

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

                                            string key = $"{NormStr(sec)}|{NormStr(twp)}|{NormStr(rge)}|{NormStr(mer)}";

                                            double area = (aabb.MaxX - aabb.MinX) * (aabb.MaxY - aabb.MinY);
                                            if (!bestByKey.TryGetValue(key, out var cur) ||
                                                area > (cur.aabb.MaxX - cur.aabb.MinX) * (cur.aabb.MaxY - cur.aabb.MinY))
                                            {
                                                bestByKey[key] = (aabb, id);
                                            }
                                        }
                                    }
                                }
                            }

                            scanned++;
                            if ((scanned % 10000) == 0) ed?.WriteMessage($"\nRESINDEXV: scanned {scanned} entities...");
                        }

                        tr.Commit();
                    }

                    // Write JSONL with all vertices of the chosen polyline per key
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? "");
                    using (var sw = new StreamWriter(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.Read)))
                    using (Transaction tr = master.TransactionManager.StartTransaction())
                    {
                        foreach (var kv in bestByKey)
                        {
                            var ent = tr.GetObject(kv.Value.entId, DbOpenMode.ForRead) as Entity;
                            if (ent == null) continue;

                            // Extract ordered vertices (WCS; flatten Z)
                            var verts = new List<Point3d>();
                            bool closed = false;

                            if (ent is Polyline pl)
                            {
                                for (int i = 0; i < pl.NumberOfVertices; i++) verts.Add(pl.GetPoint3dAt(i));
                                closed = pl.Closed;
                            }
                            else if (ent is Polyline2d pl2)
                            {
                                foreach (ObjectId vId in pl2)
                                {
                                    var v = (Vertex2d)tr.GetObject(vId, DbOpenMode.ForRead);
                                    verts.Add(v.Position);
                                }
                                closed = pl2.Closed;
                            }
                            else if (ent is Polyline3d pl3)
                            {
                                foreach (ObjectId vId in pl3)
                                {
                                    var v = (PolylineVertex3d)tr.GetObject(vId, DbOpenMode.ForRead);
                                    verts.Add(new Point3d(v.Position.X, v.Position.Y, 0));
                                }
                                closed = pl3.Closed;
                            }
                            else
                            {
                                continue;
                            }

                            var aabb = kv.Value.aabb;
                            string[] parts = kv.Key.Split('|'); // SEC|TWP|RGE|MER
                            string sec = parts[0], twp = parts[1], rge = parts[2], mer = parts[3];

                            // Build JSON (no external deps)
                            var ic = CultureInfo.InvariantCulture;
                            var sb = new System.Text.StringBuilder(256 + verts.Count * 24);
                            sb.Append('{');
                            sb.AppendFormat(ic, "\"SEC\":\"{0}\",\"TWP\":\"{1}\",\"RGE\":\"{2}\",\"MER\":\"{3}\",", sec, twp, rge, mer);
                            sb.AppendFormat(ic, "\"AABB\":{{\"minx\":{0},\"miny\":{1},\"maxx\":{2},\"maxy\":{3}}},",
                                aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY);
                            sb.AppendFormat("\"Closed\":{0},", closed ? "true" : "false");
                            sb.Append("\"Verts\":[");
                            for (int i = 0; i < verts.Count; i++)
                            {
                                if (i > 0) sb.Append(',');
                                var p = verts[i];
                                sb.AppendFormat(ic, "[{0},{1}]", p.X, p.Y);
                            }
                            sb.Append("]}");

                            sw.WriteLine(sb.ToString());
                        }
                    }

                    ed?.WriteMessage($"\nRESINDEXV: Wrote {bestByKey.Count} section outlines → {outPath}");
                }
            }
            catch (System.Exception ex)
            {
                ed?.WriteMessage($"\nRESINDEXV: Failed: {ex.Message}");
            }
            finally
            {
                if (openedHere)
                {
                    try { master.CloseAndDiscard(); } catch { /* ignore */ }
                }
            }
        }

        // =========================================================================
        // BUILDSEC — Rebuild one section exactly as scanned + insert residences
        // =========================================================================
        [CommandMethod("ResidenceSync", "BUILDSEC", CommandFlags.Modal)]
        public void BuildSectionFromVertices()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            if (!PromptSectionKey(ed, out SectionKey key)) return;

            string idxPath = Path.Combine(Path.GetDirectoryName(MASTER_SECTIONS_PATH) ?? "",
                                          "Master_Sections.index.jsonl");
            if (!File.Exists(idxPath))
            {
                ed.WriteMessage("\nBUILDSEC: Vertex index not found. Run RESINDEXV first.");
                return;
            }

            VertexIndexRecord rec;
            if (!TryReadSectionFromJsonl(idxPath, key, out rec))
            {
                ed.WriteMessage("\nBUILDSEC: Section not found in vertex index.");
                return;
            }

            using (doc.LockDocument())
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                EnsureLayer(doc.Database, "L-USEC", tr);
                EnsureLayer(doc.Database, "L-QSEC", tr);
                EnsureLayer(doc.Database, RESIDENCE_LAYER, tr);

                var bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, DbOpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

                // Outline from vertices (at master coords)
                var pl = new Polyline(rec.verts.Count);
                for (int i = 0; i < rec.verts.Count; i++)
                {
                    var p = rec.verts[i];
                    pl.AddVertexAt(i, new Point2d(p.X, p.Y), 0, 0, 0);
                }
                pl.Closed = rec.closed;
                pl.Layer = "L-USEC";
                ms.AppendEntity(pl); tr.AddNewlyCreatedDBObject(pl, true);

                if (TryGetQuarterAnchorsByEdgeMedianVertexChain(rec.verts,
                        out Point3d topV, out Point3d botV,
                        out Point3d leftV, out Point3d rightV))
                {
                    var qv = new Line(topV, botV) { Layer = "L-QSEC" };
                    var qh = new Line(leftV, rightV) { Layer = "L-QSEC" };
                    ms.AppendEntity(qv); tr.AddNewlyCreatedDBObject(qv, true);
                    ms.AppendEntity(qh); tr.AddNewlyCreatedDBObject(qh, true);
                }

                // Residences inside this polygon from master file
                var residences = ReadPointsFromMaster(out bool exists);
                if (exists && residences.Count > 0)
                {
                    int added = 0;
                    foreach (var pt in residences)
                    {
                        if (PointInPolygon2D(rec.verts, pt.X, pt.Y))
                        {
                            var dp = new DBPoint(pt) { Layer = RESIDENCE_LAYER };
                            ms.AppendEntity(dp); tr.AddNewlyCreatedDBObject(dp, true);
                            added++;
                        }
                    }
                    EnsurePointStyleVisible();
                    ed.WriteMessage($"\nBUILDSEC: Inserted {added} residence point(s).");
                }
                else
                {
                    ed.WriteMessage("\nBUILDSEC: No residences master file or none found.");
                }

                tr.Commit();
            }

            ed.WriteMessage("\nBUILDSEC: Section drawn from vertex index at master coordinates.");
        }

        // =========================================================================
        // PULLRESV — Pull residence points from master into this DWG for a section
        // =========================================================================
        [CommandMethod("ResidenceSync", "PULLRESV", CommandFlags.Modal)]
        public void PullResidencesForSection()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            if (!PromptSectionKey(ed, out SectionKey key)) return;

            string idxPath = Path.Combine(Path.GetDirectoryName(MASTER_SECTIONS_PATH) ?? "",
                                          "Master_Sections.index.jsonl");
            VertexIndexRecord rec;
            if (!TryReadSectionFromJsonl(idxPath, key, out rec))
            {
                ed.WriteMessage("\nPULLRESV: Section not found in vertex index. Run RESINDEXV first.");
                return;
            }

            // Option: clear existing points inside this section first?
            var pko = new PromptKeywordOptions("\nClear existing points inside this section first? [No/Yes]: ")
            { AllowNone = true };
            pko.Keywords.Add("No");
            pko.Keywords.Add("Yes");
            pko.Keywords.Default = "No";
            var kres = ed.GetKeywords(pko);
            bool clearInside = (kres.Status == PromptStatus.OK && string.Equals(kres.StringResult, "Yes", StringComparison.OrdinalIgnoreCase));

            using (doc.LockDocument())
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                EnsureLayer(doc.Database, RESIDENCE_LAYER, tr);

                var bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, DbOpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

                if (clearInside)
                {
                    int erased = 0;
                    foreach (ObjectId id in ms)
                    {
                        var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                        if (ent is DBPoint dp && dp.Layer == RESIDENCE_LAYER)
                        {
                            if (PointInPolygon2D(rec.verts, dp.Position.X, dp.Position.Y))
                            {
                                dp.UpgradeOpen(); dp.Erase(); erased++;
                            }
                        }
                    }
                    if (erased > 0) ed.WriteMessage($"\nPULLRESV: Cleared {erased} existing point(s) inside section.");
                }

                var residences = ReadPointsFromMaster(out bool exists);
                if (!exists)
                {
                    ed.WriteMessage("\nPULLRESV: Master residences DWG not found.");
                    tr.Commit();
                    return;
                }

                int added = 0;
                foreach (var pt in residences)
                {
                    if (PointInPolygon2D(rec.verts, pt.X, pt.Y))
                    {
                        var dp = new DBPoint(pt) { Layer = RESIDENCE_LAYER };
                        ms.AppendEntity(dp); tr.AddNewlyCreatedDBObject(dp, true);
                        added++;
                    }
                }
                EnsurePointStyleVisible();
                tr.Commit();

                ed.WriteMessage($"\nPULLRESV: Inserted {added} residence point(s) for {key}.");
            }
        }

        [CommandMethod("ResidenceSync", "PUSHRESS", CommandFlags.Modal)]
        public void PushResidencesFromScaledSection()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            // 0) Fast guard: master must be closed
            if (IsMasterPointsOpen())
            {
                ed.WriteMessage("\nPUSHRESS: 'Master_Residences.dwg' is open. Close it and try again.");
                return;
            }

            // 1) Section key (only used to read master outline & top edge)
            if (!PromptSectionKey(ed, out SectionKey key)) return;

            // 2) Load master section vertices from JSONL
            string idxPath = Path.Combine(Path.GetDirectoryName(MASTER_SECTIONS_PATH) ?? "",
                                          "Master_Sections.index.jsonl");
            if (!TryReadSectionFromJsonl(idxPath, key, out VertexIndexRecord rec))
            {
                ed.WriteMessage("\nPUSHRESS: Section not found in vertex index. Run RESINDEXV first.");
                return;
            }

            // 3) Find master top-left/top-right (for transform)
            if (!TryGetSectionTopCorners(rec.verts, out Point3d masterTopLeft, out Point3d masterTopRight))
            {
                ed.WriteMessage("\nPUSHRESS: Unable to determine top edge in master section outline.");
                return;
            }

            // 4) Pick TOP‑LEFT / TOP‑RIGHT on SCALED linework (in UCS)
            var tlRes = ed.GetPoint(new PromptPointOptions("\nPick TOP LEFT of the section in scaled linework: ")
            { AllowNone = false });
            if (tlRes.Status != PromptStatus.OK) return;

            var trRes = ed.GetPoint(new PromptPointOptions("\nPick TOP RIGHT of the section in scaled linework: ")
            {
                UseBasePoint = true,
                BasePoint = tlRes.Value,
                AllowNone = false
            });
            if (trRes.Status != PromptStatus.OK) return;

            // 5) UCS → WCS
            Matrix3d ucs = ed.CurrentUserCoordinateSystem;
            Point3d localTL_WCS = tlRes.Value.TransformBy(ucs);
            Point3d localTR_WCS = trRes.Value.TransformBy(ucs);

            // Compute uniform scale + rotation to map SCALED → MASTER
            Vector3d localVec = localTR_WCS - localTL_WCS;
            Vector3d masterVec = masterTopRight - masterTopLeft;

            double localLen = localVec.Length;
            double masterLen = masterVec.Length;
            if (localLen < 1e-9 || masterLen < 1e-9)
            {
                ed.WriteMessage("\nPUSHRESS: Degenerate corner picks.");
                return;
            }

            double scaleFactor = masterLen / localLen;
            double angleLocal = Math.Atan2(localVec.Y, localVec.X);
            double angleMaster = Math.Atan2(masterVec.Y, masterVec.X);
            double angleDelta = angleMaster - angleLocal;

            // Validate against master TR
            Point3d projectedTR = TransformScaledPoint(localTR_WCS, localTL_WCS, masterTopLeft, scaleFactor, angleDelta);
            double trErr = projectedTR.DistanceTo(new Point3d(masterTopRight.X, masterTopRight.Y, 0));
            if (trErr > TRANSFORM_VALIDATION_TOL)
            {
                ed.WriteMessage($"\nPUSHRESS: Corner picks don’t align with master top edge (err {trErr:F3} m > tol {TRANSFORM_VALIDATION_TOL:F3} m).");
                return;
            }

            ed.WriteMessage($"\nPUSHRESS: using scale={scaleFactor:F6}, rot={(angleDelta * 180.0 / Math.PI):F3}°, TR err={trErr:F3} m.");

            // 6) Select residence inputs (POINT/INSERT) from the SCALED drawing
            var sel = ed.GetSelection(
                new PromptSelectionOptions { MessageForAdding = "\nSelect residence points/blocks to push (scaled): " },
                new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "POINT,INSERT") })
            );
            if (sel.Status != PromptStatus.OK || sel.Value == null || sel.Value.Count == 0)
            {
                ed.WriteMessage("\nPUSHRESS: Nothing selected.");
                return;
            }

            // 7) Map selected insertion/base points from SCALED (WCS) → MASTER (WCS)
            var mapped = new List<Point3d>();
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in sel.Value)
                {
                    if (so?.ObjectId.IsNull != false) continue;
                    var ent = tr.GetObject(so.ObjectId, DbOpenMode.ForRead) as Entity;

                    Point3d? p = null;
                    if (ent is DBPoint dp) p = dp.Position;      // WCS
                    else if (ent is BlockReference b) p = b.Position;      // WCS
                    if (!p.HasValue) continue;

                    mapped.Add(TransformScaledPoint(p.Value, localTL_WCS, masterTopLeft, scaleFactor, angleDelta));
                }
                tr.Commit();
            }

            if (mapped.Count == 0)
            {
                ed.WriteMessage("\nPUSHRESS: No usable points from selection.");
                return;
            }

            // 8) Upsert into master (move within 3 m, else insert new)
            var result = UpsertResidencesInMaster(mapped);
            ed.WriteMessage($"\nPUSHRESS: Upsert complete — moved {result.moved}, inserted {result.inserted}. Master file saved.");
        }
        // Treat “inside OR on boundary within tol” as inside.
        // Treat “inside OR on boundary within tol” as inside.

        private static double PointToSegDist2(double px, double py,
                                              double ax, double ay,
                                              double bx, double by)
        {
            double vx = bx - ax, vy = by - ay;
            double wx = px - ax, wy = py - ay;

            double c1 = vx * wx + vy * wy;
            if (c1 <= 0.0) return (px - ax) * (px - ax) + (py - ay) * (py - ay);

            double c2 = vx * vx + vy * vy;
            if (c2 <= c1) return (px - bx) * (px - bx) + (py - by) * (py - by);

            double t = c1 / c2;
            double projx = ax + t * vx;
            double projy = ay + t * vy;
            double dx = px - projx, dy = py - projy;
            return dx * dx + dy * dy;
        }


        // Upsert all mapped points into Master_Residences.dwg:
        // - If an existing residence (DBPOINT or recognized block) is within REPLACE_TOL (3 m), move it to the new location.
        // - Otherwise insert a new DBPOINT on Z-RESIDENCE.
        // - DBPOINTs are accepted regardless of layer (no layer gating). Blocks are filtered by RES_BLOCK_NAMES.
        // Returns (#moved, #inserted). Requires the master DWG to be CLOSED.
        private (int moved, int inserted) UpsertResidencesInMaster(IEnumerable<Point3d> mappedPoints)
        {
            // Ensure target DWG exists
            if (!File.Exists(MASTER_POINTS_PATH))
            {
                using (var newDb = new Database(true, true))
                    newDb.SaveAs(MASTER_POINTS_PATH, DwgVersion.Current);
            }

            // Deduplicate input set (avoid pushing multiple times for the same spot)
            var uniqueTargets = DeduplicateList(mappedPoints.ToList(), DEDUPE_TOL);

            int moved = 0, inserted = 0;

            using (var db = new Database(false, true))
            {
                db.ReadDwgFile(MASTER_POINTS_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                db.CloseInput(true);

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    EnsureLayer(db, RESIDENCE_LAYER, tr);

                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, DbOpenMode.ForRead);
                    ObjectId resLayerId = lt[RESIDENCE_LAYER];

                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, DbOpenMode.ForRead);
                    var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

                    // Collect existing residences
                    var existing = new List<(ObjectId id, Point3d pos, bool isBlock)>();
                    foreach (ObjectId id in ms)
                    {
                        var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                        if (ent == null) continue;

                        if (ent is DBPoint dp)
                        {
                            // No layer check — include all DBPOINTs
                            existing.Add((id, dp.Position, false));
                        }
                        else if (ent is BlockReference br)
                        {
                            string name = GetEffectiveBlockName(br, tr);
                            if (!RES_BLOCK_NAMES.Contains(name)) continue;   // avoid moving non-res blocks
                            existing.Add((id, br.Position, true));
                        }
                    }

                    var existingPts = existing.Select(e => e.pos).ToList();

                    foreach (var p in uniqueTargets)
                    {
                        int idx = FindNearIndex(existingPts, p, REPLACE_TOL);
                        if (idx >= 0)
                        {
                            // Move the matched existing and remove it from the candidate pool (one-to-one mapping)
                            var ex = existing[idx];
                            var entW = tr.GetObject(ex.id, DbOpenMode.ForWrite) as Entity;

                            if (entW is DBPoint dpW)
                            {
                                dpW.Position = p;
                            }
                            else if (entW is BlockReference brW)
                            {
                                Vector3d d = p - brW.Position;
                                if (!d.IsZeroLength()) brW.TransformBy(Matrix3d.Displacement(d));
                            }

                            // consume this existing so it won't be matched again
                            existing.RemoveAt(idx);
                            existingPts.RemoveAt(idx);
                            moved++;
                        }
                        else
                        {
                            var dbp = new DBPoint(p) { LayerId = resLayerId };
                            ms.AppendEntity(dbp); tr.AddNewlyCreatedDBObject(dbp, true);
                            inserted++;
                        }
                    }

                    tr.Commit();
                }

                // Save back
                var fi = new FileInfo(MASTER_POINTS_PATH);
                if (fi.Exists && fi.IsReadOnly) fi.IsReadOnly = false;
                db.SaveAs(MASTER_POINTS_PATH, DwgVersion.Current);
            }

            return (moved, inserted);
        }

        // =========================================================================
        // SURFDEV_V4 — 4×4 development sketch
        // =========================================================================
        // =========================================================================
        // SURFDEV — 5×5 development sketch (clones residence blocks/points with OD)
        // =========================================================================
        [CommandMethod("ResidenceSync", "SURFDEV", CommandFlags.Modal)]
        public void BuildSurfaceDevelopment()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            // 1) Center section
            if (!PromptSectionKey(ed, out SectionKey centerKey)) return;

            // 2) Map scale
            var pko = new PromptKeywordOptions("\nPick scale [50k/25k/20k]: ") { AllowNone = false };
            pko.Keywords.Add("50k"); pko.Keywords.Add("25k"); pko.Keywords.Add("20k");
            var kres = ed.GetKeywords(pko);
            if (kres.Status != PromptStatus.OK) return;
            string scaleKey = (kres.StringResult ?? "50k").ToLowerInvariant();

            // 3) Surveyed vs Unsurveyed → outline layer
            var pko2 = new PromptKeywordOptions("\nIs the development Surveyed or Unsurveyed? [Surveyed/Unsurveyed]: ")
            { AllowNone = true };
            pko2.Keywords.Add("Surveyed");
            pko2.Keywords.Add("Unsurveyed");
            pko2.Keywords.Default = "Unsurveyed";
            var kres2 = ed.GetKeywords(pko2);
            bool isSurveyed = (kres2.Status == PromptStatus.OK &&
                               string.Equals(kres2.StringResult, "Surveyed", StringComparison.OrdinalIgnoreCase));
            string outlineLayer = isSurveyed ? "L-SEC" : "L-USEC";

            // 4) Insert residences?
            var pko3 = new PromptKeywordOptions("\nInsert residence objects (blocks/points) from master? [No/Yes]: ")
            { AllowNone = true };
            pko3.Keywords.Add("No");
            pko3.Keywords.Add("Yes");
            pko3.Keywords.Default = "No";
            var kres3 = ed.GetKeywords(pko3);
            bool insertResidences = (kres3.Status == PromptStatus.OK &&
                                     string.Equals(kres3.StringResult, "Yes", StringComparison.OrdinalIgnoreCase));

            // 5) Insertion point
            var pIns = ed.GetPoint("\nPick insertion point (centre of middle section): ");
            if (pIns.Status != PromptStatus.OK) return;
            Point3d insertCenter = pIns.Value;

            // Units/scales
            double unitsPerKm = (scaleKey == "25k") ? 200.0 : (scaleKey == "20k") ? 250.0 : 100.0;
            double unitsPerMetre = unitsPerKm / 1000.0;
            double secTextHt = (scaleKey == "50k") ? 15.0 : (scaleKey == "25k") ? 30.0 : 37.5;

            // Index path
            string idxPath = Path.Combine(Path.GetDirectoryName(MASTER_SECTIONS_PATH) ?? "",
                                          "Master_Sections.index.jsonl");
            if (!File.Exists(idxPath))
            {
                ed.WriteMessage("\nSURFDEV: Vertex index not found. Run RESINDEXV first.");
                return;
            }

            // Load center record
            if (!TryReadSectionFromJsonl(idxPath, centerKey, out VertexIndexRecord centerRec))
            {
                ed.WriteMessage("\nSURFDEV: Center section not found in vertex index.");
                return;
            }

            // MASTER centre (AABB centre)
            Point3d centerMm = new Point3d(
                (centerRec.aabb.MinX + centerRec.aabb.MaxX) * 0.5,
                (centerRec.aabb.MinY + centerRec.aabb.MaxY) * 0.5, 0);

            // MASTER → SKETCH map
            Func<Point3d, Point3d> Map = pm =>
                insertCenter + new Vector3d((pm.X - centerMm.X) * unitsPerMetre,
                                            (pm.Y - centerMm.Y) * unitsPerMetre, 0);

            // Precompute global transform for cloned entities
            Matrix3d xform = BuildMasterToSketchTransform(centerMm, insertCenter, unitsPerMetre);

            // DLS serpentine (row 0=north, col 0=west)
            int[,] serp =
            {
        {31,32,33,34,35,36},
        {30,29,28,27,26,25},
        {19,20,21,22,23,24},
        {18,17,16,15,14,13},
        { 7, 8, 9,10,11,12},
        { 6, 5, 4, 3, 2, 1}
    };

            // Locate center
            if (!int.TryParse(centerKey.Section.TrimStart('0'), out int secNum))
            { ed.WriteMessage("\nSURFDEV: SEC not numeric."); return; }

            int selRow = -1, selCol = -1;
            for (int r = 0; r < 6 && selRow < 0; r++)
                for (int c = 0; c < 6; c++)
                    if (serp[r, c] == secNum) { selRow = r; selCol = c; break; }
            if (selRow < 0) { ed.WriteMessage("\nSURFDEV: SEC not in 1..36."); return; }

            // Neighbor key (Township↑ north, Range↑ west)
            SectionKey NeighborKey(int dRow, int dCol)
            {
                int targetRow = selRow + dRow;
                int targetCol = selCol + dCol;

                int twp; int rge;
                if (!int.TryParse(centerKey.Township.Trim(), out twp)) twp = 0;
                if (!int.TryParse(centerKey.Range.Trim(), out rge)) rge = 0;

                while (targetRow < 0) { twp += 1; targetRow += 6; }
                while (targetRow > 5) { twp -= 1; targetRow -= 6; }
                while (targetCol < 0) { rge += 1; targetCol += 6; }
                while (targetCol > 5) { rge -= 1; targetCol -= 6; }

                if (twp <= 0 || rge <= 0) return default(SectionKey);

                int s = serp[targetRow, targetCol];
                return new SectionKey(s.ToString(),
                                      twp.ToString(CultureInfo.InvariantCulture),
                                      rge.ToString(CultureInfo.InvariantCulture),
                                      centerKey.Meridian);
            }

            // Build the 5×5 in one shot and (optionally) collect the master entities to clone.
            using (doc.LockDocument())
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Layers
                EnsureLayer(doc.Database, outlineLayer, tr);
                EnsureLayer(doc.Database, "L-QSEC", tr);
                EnsureLayer(doc.Database, "S-7", tr);
                if (insertResidences) EnsureLayer(doc.Database, RESIDENCE_LAYER, tr);

                // Force colors on layers (and set entities explicitly)
                SetLayerColor(doc.Database, outlineLayer, 253, tr);
                SetLayerColor(doc.Database, "L-QSEC", 253, tr);

                var bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, DbOpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForWrite);

                // Store polygons for the 5×5 (in master coords) so we can test master entities.
                var sectionPolysMaster = new List<List<Point3d>>();
                int secCount = 0;

                // ---- 5×5 window around centre: offsets -2..+2 ----
                for (int dRow = -2; dRow <= 2; dRow++)
                {
                    for (int dCol = -2; dCol <= 2; dCol++)
                    {
                        SectionKey k2 = NeighborKey(dRow, dCol);
                        if (string.IsNullOrEmpty(k2.Section)) continue;

                        if (!TryReadSectionFromJsonl(idxPath, k2, out VertexIndexRecord rec))
                            continue;

                        // Keep the master poly for hit-testing master entities later
                        sectionPolysMaster.Add(rec.verts);

                        // Outline
                        var pl = new Polyline(rec.verts.Count);
                        for (int i = 0; i < rec.verts.Count; i++)
                        {
                            var p = Map(rec.verts[i]);
                            pl.AddVertexAt(i, new Point2d(p.X, p.Y), 0, 0, 0);
                        }
                        pl.Closed = rec.closed;
                        pl.Layer = outlineLayer;
                        pl.ColorIndex = 253;
                        ms.AppendEntity(pl); tr.AddNewlyCreatedDBObject(pl, true);

                        // Quarter-line anchors
                        if (TryGetQuarterAnchorsByEdgeMedianVertexChain(rec.verts,
                                out Point3d topV, out Point3d botV, out Point3d leftV, out Point3d rightV))
                        {
                            Point3d topS = Map(topV);
                            Point3d botS = Map(botV);
                            Point3d leftS = Map(leftV);
                            Point3d rightS = Map(rightV);

                            var qv = new Line(topS, botS) { Layer = "L-QSEC", ColorIndex = 253 };
                            var qh = new Line(leftS, rightS) { Layer = "L-QSEC", ColorIndex = 253 };
                            ms.AppendEntity(qv); tr.AddNewlyCreatedDBObject(qv, true);
                            ms.AppendEntity(qh); tr.AddNewlyCreatedDBObject(qh, true);

                            // Label at intersection (S-7)
                            Point3d centerS = new Point3d(
                                (topS.X + botS.X + leftS.X + rightS.X) * 0.25,
                                (topS.Y + botS.Y + leftS.Y + rightS.Y) * 0.25, 0);
                            AddMaskedLabel(ms, tr, centerS, k2.Section.TrimStart('0'), secTextHt, "S-7");
                        }
                        else
                        {
                            // Fallback: label at AABB centre
                            double lx = double.MaxValue, ly = double.MaxValue, ux = double.MinValue, uy = double.MinValue;
                            foreach (var v in rec.verts)
                            {
                                var p = Map(v);
                                if (p.X < lx) lx = p.X; if (p.Y < ly) ly = p.Y;
                                if (p.X > ux) ux = p.X; if (p.Y > uy) uy = p.Y;
                            }
                            Point3d ctr2 = new Point3d((lx + ux) * 0.5, (ly + uy) * 0.5, 0);
                            AddMaskedLabel(ms, tr, ctr2, k2.Section.TrimStart('0'), secTextHt, "S-7");
                        }

                        secCount++;
                    }
                }

                int inserted = 0;

                if (insertResidences)
                {
                    if (!File.Exists(MASTER_POINTS_PATH))
                    {
                        ed.WriteMessage("\nSURFDEV: Master residences DWG not found; skipping residence insertion.");
                    }
                    else
                    {
                        // Open the master file (database only) and collect the source ids to clone.
                        using (var masterDb = new Database(false, true))
                        {
                            masterDb.ReadDwgFile(MASTER_POINTS_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                            masterDb.CloseInput(true);

                            ObjectIdCollection srcIds = CollectResidenceSourceIds(masterDb, sectionPolysMaster);

                            if (srcIds.Count > 0)
                            {
                                // Clone the master entities into THIS drawing's ModelSpace.
                                var map = new IdMapping();
                                doc.Database.WblockCloneObjects(
                                    srcIds, ms.ObjectId, map, DuplicateRecordCloning.Replace, false);

                                // Transform each newly cloned entity into the sketch frame and set its layer.
                                foreach (IdPair pair in map)
                                {
                                    if (!pair.IsCloned) continue;
                                    var ent = tr.GetObject(pair.Value, DbOpenMode.ForWrite) as Entity;
                                    if (ent == null) continue;
                                    ent.TransformBy(xform);
                                    ent.Layer = RESIDENCE_LAYER;       // force them onto Z-RESIDENCE
                                    inserted++;
                                }

                                if (inserted > 0) EnsurePointStyleVisible(); // in case some are DBPOINTs
                            }
                        }
                    }
                }

                tr.Commit();

                ed.WriteMessage($"\nSURFDEV: Built 5×5 ({secCount} sections){(insertResidences ? $", inserted {inserted} residence object(s) (blocks/points, OD preserved)" : "")}. Outlines/Q-sec color 253; labels on S-7 with mask.");
            }

            ed.Regen();
        }
        // ---- NEW: simple open-file guard ----
        private bool IsMasterPointsOpen()
        {
            return GetOpenDocumentByPath(AcadApp.DocumentManager, MASTER_POINTS_PATH) != null;
        }

        // =========================================================================
        // Helpers (layers, labels, chains, JSONL, geometry)
        // =========================================================================

        private static bool IsPolylineEntity(Entity entity)
            => entity is Polyline || entity is Polyline2d || entity is Polyline3d;

        private List<string> BuildOdTableSearchOrder(Tables tables)
        {
            var names = new List<string>();
            if (tables == null) return names;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(PREFERRED_OD_TABLE) && tables.IsTableDefined(PREFERRED_OD_TABLE))
            {
                names.Add(PREFERRED_OD_TABLE);
                seen.Add(PREFERRED_OD_TABLE);
            }

            foreach (string name in tables.GetTableNames())
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (seen.Add(name)) names.Add(name);
            }
            return names;
        }

        private static Document GetOpenDocumentByPath(DocumentCollection docCollection, string fullPath)
        {
            if (docCollection == null || string.IsNullOrWhiteSpace(fullPath)) return null;
            string normalizedTarget = NormalizePath(fullPath);
            foreach (Document openDoc in docCollection)
            {
                if (openDoc == null) continue;
                string docPath = NormalizePath(openDoc.Name);
                if (!string.IsNullOrEmpty(docPath) &&
                    string.Equals(docPath, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                    return openDoc;
            }
            return null;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try { return Path.GetFullPath(path); } catch { return path; }
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
                    var ext = ent.GeometricExtents; // robust to rotation, bulges
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
                        for (int i = 0; i < pl.NumberOfVertices; i++)
                        {
                            var p = pl.GetPoint3dAt(i);
                            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
                            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
                        }
                    }
                    else if (ent is Polyline2d pl2)
                    {
                        foreach (ObjectId vId in pl2)
                        {
                            var v = (Vertex2d)tr.GetObject(vId, DbOpenMode.ForRead);
                            var p = v.Position;
                            if (p.X < minX) minX = p.X; if (p.Y < minY) minY = p.Y;
                            if (p.X > maxX) maxX = p.X; if (p.Y > maxY) maxY = p.Y;
                        }
                    }
                    else if (ent is Polyline3d pl3)
                    {
                        foreach (ObjectId vId in pl3)
                        {
                            var v = (PolylineVertex3d)tr.GetObject(vId, DbOpenMode.ForRead);
                            var p = new Point3d(v.Position.X, v.Position.Y, 0);
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

        private static void EnsurePointStyleVisible()
        {
            try
            {
                AcadApp.SetSystemVariable("PDMODE", 3);
                AcadApp.SetSystemVariable("PDSIZE", 0.8);
            }
            catch { /* ignore if locked */ }
        }

        private void EnsureLayer(Database db, string layerName, Transaction tr)
        {
            var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, DbOpenMode.ForRead);
            if (!layerTable.Has(layerName))
            {
                layerTable.UpgradeOpen();
                var layerRecord = new LayerTableRecord { Name = layerName };
                layerTable.Add(layerRecord);
                tr.AddNewlyCreatedDBObject(layerRecord, true);
            }
        }

        // Force a layer's ACI color
        private void SetLayerColor(Database db, string layerName, short aci, Transaction tr)
        {
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, DbOpenMode.ForRead);
            if (!lt.Has(layerName)) return;
            var ltr = (LayerTableRecord)tr.GetObject(lt[layerName], DbOpenMode.ForWrite);
            ltr.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                Autodesk.AutoCAD.Colors.ColorMethod.ByAci, aci);
        }

        // Masked label (MText with background fill)
        private void AddMaskedLabel(BlockTableRecord ms, Transaction tr,
                                    Point3d pos, string text, double height, string layer)
        {
            var mt = new MText
            {
                Location = pos,
                Attachment = AttachmentPoint.MiddleCenter,
                TextHeight = height,
                Contents = text,
                Layer = layer,
                ColorIndex = 256   // ByLayer for S-7
            };
            mt.BackgroundFill = true;
            mt.UseBackgroundColor = true;
            mt.BackgroundScaleFactor = 1.25;

            ms.AppendEntity(mt);
            tr.AddNewlyCreatedDBObject(mt, true);
        }

        // ---------- Side-chain selection (robust) ----------

        private struct EdgeInfo
        {
            public int Index;           // segment index (start vertex index)
            public Point3d A, B, Mid;   // endpoints and midpoint
            public Vector3d U;          // unit direction
            public double Len;          // length
        }

        private struct ChainInfo
        {
            public int Start;           // starting segment index
            public int SegCount;        // number of contiguous segments
            public double Score;        // average projection onto the orthogonal axis (north/east)
            public double TotalLen;     // total length of the chain
        }

        // Build contiguous chains by whichever axis an edge is closer to (no hard angle cutoff).
        private static List<ChainInfo> BuildChainsClosest(List<EdgeInfo> edges, Vector3d primary, Vector3d other)
        {
            var chains = new List<ChainInfo>();
            bool inChain = false;
            int start = -1;
            double sumProj = 0.0;
            int cnt = 0;
            double totLen = 0.0;

            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                double de = Math.Abs(e.U.DotProduct(primary));
                double dn = Math.Abs(e.U.DotProduct(other));
                bool isPrimary = de >= dn;

                if (isPrimary)
                {
                    if (!inChain) { inChain = true; start = e.Index; sumProj = 0.0; cnt = 0; totLen = 0.0; }
                    sumProj += (e.Mid - Point3d.Origin).DotProduct(other);
                    cnt++;
                    totLen += e.Len;
                }
                else
                {
                    if (inChain)
                    {
                        chains.Add(new ChainInfo { Start = start, SegCount = cnt, Score = (cnt > 0 ? sumProj / cnt : 0.0), TotalLen = totLen });
                        inChain = false;
                    }
                }
            }
            if (inChain)
                chains.Add(new ChainInfo { Start = start, SegCount = cnt, Score = (cnt > 0 ? sumProj / cnt : 0.0), TotalLen = totLen });

            // Merge wrap-around contiguous chains (last + first)
            if (chains.Count >= 2)
            {
                var first = chains[0];
                var last = chains[chains.Count - 1];
                if (first.Start == 0 && (last.Start + last.SegCount == edges.Count))
                {
                    int totalSeg = last.SegCount + first.SegCount;
                    double totalLen = last.TotalLen + first.TotalLen;
                    double avgScore = (totalSeg > 0) ? (last.Score * last.SegCount + first.Score * first.SegCount) / totalSeg : 0.0;
                    var merged = new ChainInfo { Start = last.Start, SegCount = totalSeg, Score = avgScore, TotalLen = totalLen };
                    chains[0] = merged;
                    chains.RemoveAt(chains.Count - 1);
                }
            }
            return chains;
        }

        // Helper: iterate vertex indices across a chain (m segments → m+1 vertices)
        // ---- helpers used by the anchor picker ----
        private static IEnumerable<int> ChainVertexIndices(ChainInfo ch, int vertexCount)
        {
            for (int k = 0; k <= ch.SegCount; k++)
                yield return (ch.Start + k) % vertexCount;
        }
        private static double AxisProj(Point3d p, Vector3d axis)
        {
            return (p - Point3d.Origin).DotProduct(axis);
        }

        // Pick the median existing vertex in a chain w.r.t. a given axis (E for top/bottom).
        private static Point3d ChainMedianByAxis(List<Point3d> verts, ChainInfo ch, Vector3d axis)
        {
            var idxs = ChainVertexIndices(ch, verts.Count)
                       .OrderBy(i => AxisProj(verts[i], axis))
                       .ToList();
            return verts[idxs[idxs.Count / 2]];
        }

        // Pick the existing vertex in a chain whose projection on `axis` is nearest to `target`.
        private static Point3d ChainVertexNearestAxisValue(List<Point3d> verts, ChainInfo ch, Vector3d axis, double target)
        {
            int bestIdx = (ch.Start) % verts.Count;
            double best = double.MaxValue;

            foreach (int i in ChainVertexIndices(ch, verts.Count))
            {
                double d = Math.Abs(AxisProj(verts[i], axis) - target);
                if (d < best) { best = d; bestIdx = i; }
            }
            return verts[bestIdx];
        }



        // Returns the middle existing vertex (median-by-coordinate) from each true side chain.
        private static bool TryGetQuarterAnchorsByEdgeMedianVertexChain(
            List<Point3d> verts,
            out Point3d topV, out Point3d bottomV, out Point3d leftV, out Point3d rightV)
        {
            topV = bottomV = leftV = rightV = Point3d.Origin;
            if (verts == null || verts.Count < 3) return false;

            // Build edges
            int n = verts.Count;
            var edges = new List<EdgeInfo>(n);
            for (int i = 0; i < n; i++)
            {
                Point3d a = verts[i];
                Point3d b = verts[(i + 1) % n];
                Vector3d v = b - a;
                double len = v.Length;
                if (len <= 1e-9) continue;
                Vector3d u = new Vector3d(v.X / len, v.Y / len, 0);
                Point3d mid = new Point3d((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5, 0);
                edges.Add(new EdgeInfo { Index = i, A = a, B = b, U = u, Mid = mid, Len = len });
            }
            if (edges.Count == 0) return false;

            // Local axes from the highest near-horizontal edge
            const double degTol = 15.0;
            double cosTol = Math.Cos(degTol * Math.PI / 180.0);
            EdgeInfo topEdge = default(EdgeInfo);
            double bestTopY = double.MinValue;
            foreach (var e in edges)
            {
                double horiz = Math.Abs(e.U.DotProduct(Vector3d.XAxis));
                double avgY = (e.A.Y + e.B.Y) * 0.5;
                if (horiz >= cosTol && avgY > bestTopY)
                {
                    bestTopY = avgY; topEdge = e;
                }
            }
            if (bestTopY == double.MinValue)
                topEdge = edges.OrderByDescending(e => e.Len).First();

            Vector3d east = topEdge.U.GetNormal();
            if (east.Length <= 1e-12) return false;
            Vector3d north = east.RotateBy(Math.PI / 2.0, Vector3d.ZAxis).GetNormal();

            // Precompute projections & extremes (AABB in local frame)
            double minE = double.MaxValue, maxE = double.MinValue;
            double minN = double.MaxValue, maxN = double.MinValue;
            for (int i = 0; i < n; i++)
            {
                Vector3d dp = verts[i] - Point3d.Origin;
                double pe = dp.DotProduct(east);
                double pn = dp.DotProduct(north);
                if (pe < minE) minE = pe; if (pe > maxE) maxE = pe;
                if (pn < minN) minN = pn; if (pn > maxN) maxN = pn;
            }
            double spanE = Math.Max(1e-6, maxE - minE);
            double spanN = Math.Max(1e-6, maxN - minN);
            double bandTol = Math.Max(2.0, 0.005 * Math.Max(spanE, spanN)); // 2 m or 0.5%

            // Chains by closest axis
            var eChains = BuildChainsClosest(edges, east, north); // top/bottom
            var nChains = BuildChainsClosest(edges, north, east); // left/right
            if (eChains.Count == 0 || nChains.Count == 0) return false;

            // band gates
            bool TouchesTop(ChainInfo ch)
            {
                for (int k = 0; k <= ch.SegCount; k++)
                {
                    int idx = (ch.Start + k) % n;
                    Vector3d d = verts[idx] - Point3d.Origin;
                    if (maxN - d.DotProduct(north) <= bandTol) return true;
                }
                return false;
            }
            bool TouchesBottom(ChainInfo ch)
            {
                for (int k = 0; k <= ch.SegCount; k++)
                {
                    int idx = (ch.Start + k) % n;
                    Vector3d d = verts[idx] - Point3d.Origin;
                    if (d.DotProduct(north) - minN <= bandTol) return true;
                }
                return false;
            }
            bool TouchesLeft(ChainInfo ch)
            {
                for (int k = 0; k <= ch.SegCount; k++)
                {
                    int idx = (ch.Start + k) % n;
                    Vector3d d = verts[idx] - Point3d.Origin;
                    if (d.DotProduct(east) - minE <= bandTol) return true;
                }
                return false;
            }
            bool TouchesRight(ChainInfo ch)
            {
                for (int k = 0; k <= ch.SegCount; k++)
                {
                    int idx = (ch.Start + k) % n;
                    Vector3d d = verts[idx] - Point3d.Origin;
                    if (maxE - d.DotProduct(east) <= bandTol) return true;
                }
                return false;
            }

            // choose extremes with length tie-break
            ChainInfo PickMax(List<ChainInfo> list, Func<ChainInfo, bool> gate) =>
                list.Where(gate).OrderByDescending(c => c.Score).ThenByDescending(c => c.TotalLen)
                    .DefaultIfEmpty(list.OrderByDescending(c => c.Score).ThenByDescending(c => c.TotalLen).First())
                    .First();

            ChainInfo PickMin(List<ChainInfo> list, Func<ChainInfo, bool> gate) =>
                list.Where(gate).OrderBy(c => c.Score).ThenByDescending(c => c.TotalLen)
                    .DefaultIfEmpty(list.OrderBy(c => c.Score).ThenByDescending(c => c.TotalLen).First())
                    .First();

            var top = PickMax(eChains, TouchesTop);
            var bottom = PickMin(eChains, TouchesBottom);
            var left = PickMin(nChains, TouchesLeft);
            var right = PickMax(nChains, TouchesRight);

            // **Median-by-coordinate** vertex on each winning chain
            // Replace the four lines at the end of TryGetQuarterAnchorsByEdgeMedianVertexChain with:

            // Top & Bottom: median existing vertex along the local EAST axis
            topV = ChainMedianByAxis(verts, top, east);
            bottomV = ChainMedianByAxis(verts, bottom, east);

            // Shared "horizontal" in local NORTH to align the left/right endpoints
            double N_target = 0.5 * (AxisProj(topV, north) + AxisProj(bottomV, north));

            // Left & Right: existing vertex on each chain closest to that same NORTH value
            leftV = ChainVertexNearestAxisValue(verts, left, north, N_target);
            rightV = ChainVertexNearestAxisValue(verts, right, north, N_target);

            return true;
        }



        // Recognized residence block names in the master (case-insensitive).
        private static readonly HashSet<string> RES_BLOCK_NAMES =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "AS-RESIDENCE", "RES_ABD", "RES_OTHER" };

        // Effective (base) name for dynamic or regular blocks.
        private static string GetEffectiveBlockName(BlockReference br, Transaction tr)
        {
            ObjectId btrId = br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord;
            var btr = (BlockTableRecord)tr.GetObject(btrId, DbOpenMode.ForRead);
            return btr?.Name ?? string.Empty;
        }

        // Pick up master entities (block refs / dbpoints) whose insertion point lies inside any of the polygons.
        private ObjectIdCollection CollectResidenceSourceIds(Database masterDb, List<List<Point3d>> sectionPolys)
        {
            var outIds = new ObjectIdCollection();
            if (sectionPolys == null || sectionPolys.Count == 0) return outIds;

            using (var tr = masterDb.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, DbOpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                foreach (ObjectId id in ms)
                {
                    var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                    if (ent == null) continue;

                    Point3d pos;
                    bool isResidence = false;

                    if (ent is BlockReference br)
                    {
                        string name = GetEffectiveBlockName(br, tr);
                        if (!RES_BLOCK_NAMES.Contains(name)) continue; // only our residence blocks
                        pos = br.Position;
                        isResidence = true;
                    }
                    else if (ent is DBPoint dp)
                    {
                        // Legacy points—allow as fallback.
                        pos = dp.Position;
                        isResidence = true;
                    }
                    else
                    {
                        continue;
                    }

                    if (!isResidence) continue;

                    // Inside any of the 5×5 section polygons?
                    foreach (var poly in sectionPolys)
                    {
                        if (PointInPolygon2D(poly, pos.X, pos.Y))
                        {
                            outIds.Add(id);
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return outIds;
        }

        // Build the same mapping transform used by the outlines: scale about center, then shift.
        private static Matrix3d BuildMasterToSketchTransform(Point3d centerMm, Point3d insertCenter, double unitsPerMetre)
        {
            // p' = insertCenter + unitsPerMetre * (p - centerMm)
            var scaleAboutCenter = Matrix3d.Scaling(unitsPerMetre, centerMm);
            var postShift = Matrix3d.Displacement(
                new Vector3d(insertCenter.X - centerMm.X * unitsPerMetre,
                             insertCenter.Y - centerMm.Y * unitsPerMetre, 0));
            return scaleAboutCenter * postShift;
        }

        private struct VertexIndexRecord
        {
            public string sec, twp, rge, mer;
            public (double MinX, double MinY, double MaxX, double MaxY) aabb;
            public List<Point3d> verts;
            public bool closed;
        }

        private bool TryReadSectionFromJsonl(string jsonlPath, SectionKey key, out VertexIndexRecord rec)
        {
            rec = default;
            var ic = CultureInfo.InvariantCulture;

            using (var fs = new FileStream(jsonlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!TryParseJsonLine(line, out var r)) continue;

                    if (EqNorm(r.sec, key.Section) &&
                        EqNorm(r.twp, key.Township) &&
                        EqNorm(r.rge, key.Range) &&
                        EqNorm(r.mer, key.Meridian))
                    {
                        rec = r;
                        return true;
                    }
                }
            }
            return false;

            bool TryParseJsonLine(string s, out VertexIndexRecord r)
            {
                r = default;
                try
                {
                    string GetStr(string tag)
                    {
                        int idxTag = s.IndexOf($"\"{tag}\"", StringComparison.OrdinalIgnoreCase);
                        if (idxTag < 0) return null;
                        int colon = s.IndexOf(':', idxTag); if (colon < 0) return null;
                        int q1 = s.IndexOf('\"', colon + 1); if (q1 < 0) return null;
                        int q2 = s.IndexOf('\"', q1 + 1); if (q2 < 0) return null;
                        return s.Substring(q1 + 1, q2 - q1 - 1);
                    }

                    string sec = GetStr("SEC");
                    string twp = GetStr("TWP");
                    string rge = GetStr("RGE");
                    string mer = GetStr("MER");

                    double minx = ExtractNumberAfter(s, "\"minx\"");
                    double miny = ExtractNumberAfter(s, "\"miny\"");
                    double maxx = ExtractNumberAfter(s, "\"maxx\"");
                    double maxy = ExtractNumberAfter(s, "\"maxy\"");

                    bool closed = s.IndexOf("\"Closed\":true", StringComparison.OrdinalIgnoreCase) >= 0;

                    int iv = s.IndexOf("\"Verts\":[", StringComparison.OrdinalIgnoreCase);
                    if (iv < 0) return false;
                    int start = s.IndexOf('[', iv + 8);
                    int end = s.LastIndexOf(']');
                    if (start < 0 || end < 0 || end <= start) return false;

                    var verts = new List<Point3d>();
                    int idx = start + 1;
                    while (idx < end)
                    {
                        int a = s.IndexOf('[', idx);
                        if (a < 0 || a > end) break;
                        int b = s.IndexOf(']', a);
                        if (b < 0 || b > end) break;
                        string pair = s.Substring(a + 1, b - a - 1);
                        int comma = pair.IndexOf(',');
                        if (comma > 0)
                        {
                            string sx = pair.Substring(0, comma).Trim();
                            string sy = pair.Substring(comma + 1).Trim();
                            if (double.TryParse(sx, NumberStyles.Float, ic, out double x) &&
                                double.TryParse(sy, NumberStyles.Float, ic, out double y))
                            {
                                verts.Add(new Point3d(x, y, 0));
                            }
                        }
                        idx = b + 1; // advance
                    }

                    r = new VertexIndexRecord
                    {
                        sec = NormStr(sec),
                        twp = NormStr(twp),
                        rge = NormStr(rge),
                        mer = NormStr(mer),
                        aabb = (minx, miny, maxx, maxy),
                        verts = verts,
                        closed = closed
                    };
                    return (verts.Count >= 3);
                }
                catch { return false; }
            }

            double ExtractNumberAfter(string s2, string tag)
            {
                int posTag = s2.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (posTag < 0) return 0;
                int colon = s2.IndexOf(':', posTag);
                if (colon < 0) return 0;
                int startNum = colon + 1;
                while (startNum < s2.Length && char.IsWhiteSpace(s2[startNum])) startNum++;
                int endNum = startNum;
                while (endNum < s2.Length &&
                       (char.IsDigit(s2[endNum]) || s2[endNum] == '.' || s2[endNum] == '-' ||
                        s2[endNum] == 'e' || s2[endNum] == 'E' || s2[endNum] == '+'))
                {
                    endNum++;
                }
                double v;
                if (double.TryParse(s2.Substring(startNum, endNum - startNum), NumberStyles.Float,
                                    CultureInfo.InvariantCulture, out v))
                    return v;
                return 0;
            }
        }



        // -------- ObjectData read helpers & string normalizers --------

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

        private string MapValueToString(MapValue mv)
        {
            if (mv == null) return null;
            switch (mv.Type)
            {
                case OdDataType.Character: return mv.StrValue;
                case OdDataType.Integer: return mv.Int32Value.ToString(CultureInfo.InvariantCulture);
                case OdDataType.Real: return mv.DoubleValue.ToString("0.####", CultureInfo.InvariantCulture);
                default: return null;
            }
        }

        private static string NormStr(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            string trimmed = s.Trim();
            if (int.TryParse(trimmed, out int n)) return n.ToString(CultureInfo.InvariantCulture);
            string noZeros = trimmed.TrimStart('0');
            return noZeros.Length > 0 ? noZeros : "0";
        }

        private static bool EqNorm(string a, string b)
        {
            if (int.TryParse(a?.Trim(), out int ai) && int.TryParse(b?.Trim(), out int bi)) return ai == bi;
            string aa = NormStr(a);
            string bb = NormStr(b);
            if (int.TryParse(aa, out ai) && int.TryParse(bb, out bi)) return ai == bi;
            return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // --------- User prompts & master points reader ---------

        private bool PromptSectionKey(Editor ed, out SectionKey key)
        {
            key = default;

            string sec = PromptString(ed, "Enter SEC: ");
            if (sec == null) return false;

            string twp = PromptString(ed, "Enter TWP: ");
            if (twp == null) return false;

            string rge = PromptString(ed, "Enter RGE: ");
            if (rge == null) return false;

            string mer = PromptString(ed, "Enter MER: ");
            if (mer == null) return false;

            key = new SectionKey(sec, twp, rge, mer);
            return true;
        }

        private string PromptString(Editor ed, string message)
        {
            var opts = new PromptStringOptions("\n" + message) { AllowSpaces = false };
            var res = ed.GetString(opts);
            return (res.Status == PromptStatus.OK) ? res.StringResult : null;
        }

        private List<Point3d> ReadPointsFromMaster(out bool exists)
        {
            exists = File.Exists(MASTER_POINTS_PATH);
            if (!exists) return new List<Point3d>();

            var points = new List<Point3d>();
            using (var masterDb = new Database(false, true))
            {
                masterDb.ReadDwgFile(MASTER_POINTS_PATH, FileOpenMode.OpenForReadAndAllShare, false, null);
                masterDb.CloseInput(true);

                using (Transaction tr = masterDb.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(masterDb.BlockTableId, DbOpenMode.ForRead);
                    var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], DbOpenMode.ForRead);

                    foreach (ObjectId id in ms)
                    {
                        var ent = tr.GetObject(id, DbOpenMode.ForRead) as Entity;
                        if (ent == null) continue;

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

        // --------- Geometry & utilities ---------

        // Robustly find TOP-LEFT (NW) and TOP-RIGHT (NE) corners of the master section.
        // Works even when the top edge has a slight slope and vertex spacing is irregular.
        // Robustly find TOP-LEFT (NW) and TOP-RIGHT (NE) corners of the master section.
        // Works even when the top edge has a slight slope and vertex spacing is irregular.
        private bool TryGetSectionTopCorners(List<Point3d> verts, out Point3d topLeft, out Point3d topRight)
        {
            topLeft = topRight = Point3d.Origin;
            if (verts == null || verts.Count < 3) return false;

            // Build edges
            int n = verts.Count;
            var edges = new List<EdgeInfo>(n);
            for (int i = 0; i < n; i++)
            {
                Point3d a = verts[i];
                Point3d b = verts[(i + 1) % n];
                Vector3d v = b - a;
                double len = v.Length;
                if (len <= 1e-9) continue;
                Vector3d u = new Vector3d(v.X / len, v.Y / len, 0);
                Point3d mid = new Point3d((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5, 0);
                edges.Add(new EdgeInfo { Index = i, A = a, B = b, U = u, Mid = mid, Len = len });
            }
            if (edges.Count == 0) return false;

            // Choose a near-horizontal edge with highest average Y; fallback = longest edge.
            const double degTol = 15.0;
            double cosTol = Math.Cos(degTol * Math.PI / 180.0);
            EdgeInfo topEdge = default(EdgeInfo);
            double bestTopY = double.MinValue;
            foreach (var e in edges)
            {
                double horiz = Math.Abs(e.U.DotProduct(Vector3d.XAxis));
                double avgY = (e.A.Y + e.B.Y) * 0.5;
                if (horiz >= cosTol && avgY > bestTopY) { bestTopY = avgY; topEdge = e; }
            }
            if (bestTopY == double.MinValue)
                topEdge = edges.OrderByDescending(e => e.Len).First();

            // Local axes: EAST along the top edge, NORTH perpendicular
            Vector3d east = topEdge.U.GetNormal();
            if (east.Length <= 1e-12) return false;
            Vector3d north = east.RotateBy(Math.PI / 2.0, Vector3d.ZAxis).GetNormal();

            // Extents & band tolerance (2 m or 0.5% of the larger span)
            double minE = double.MaxValue, maxE = double.MinValue;
            double minN = double.MaxValue, maxN = double.MinValue;
            foreach (var v in verts)
            {
                double pe = AxisProj(v, east);
                double pn = AxisProj(v, north);
                if (pe < minE) minE = pe; if (pe > maxE) maxE = pe;
                if (pn < minN) minN = pn; if (pn > maxN) maxN = pn;
            }
            double spanE = Math.Max(1e-6, maxE - minE);
            double spanN = Math.Max(1e-6, maxN - minN);
            double bandTol = Math.Max(2.0, 0.005 * Math.Max(spanE, spanN));

            // Build east-west chains and pick the one touching the top band
            var eChains = BuildChainsClosest(edges, east, north);
            if (eChains.Count == 0) return false;

            bool TouchesTop(ChainInfo ch)
            {
                foreach (int idx in ChainVertexIndices(ch, n))
                    if (maxN - AxisProj(verts[idx], north) <= bandTol)
                        return true;
                return false;
            }

            ChainInfo topChain = eChains
                .Where(TouchesTop)
                .OrderByDescending(c => c.Score)       // most "northern"
                .ThenByDescending(c => c.TotalLen)     // tie-break by length
                .DefaultIfEmpty(eChains.OrderByDescending(c => c.Score).ThenByDescending(c => c.TotalLen).First())
                .First();

            // The corners are the min/max EAST vertices along that top chain
            int leftIdx = topChain.Start % n, rightIdx = leftIdx;
            double bestLeftE = double.MaxValue, bestRightE = double.MinValue;
            foreach (int idx in ChainVertexIndices(topChain, n))
            {
                double pe = AxisProj(verts[idx], east);
                if (pe < bestLeftE) { bestLeftE = pe; leftIdx = idx; }
                if (pe > bestRightE) { bestRightE = pe; rightIdx = idx; }
            }

            topLeft = verts[leftIdx];
            topRight = verts[rightIdx];
            return true;
        }

        private static Point3d TransformScaledPoint(Point3d source, Point3d localOrigin, Point3d masterOrigin, double scale, double rotation)
        {
            double dx = source.X - localOrigin.X;
            double dy = source.Y - localOrigin.Y;

            double cos = Math.Cos(rotation);
            double sin = Math.Sin(rotation);

            double mx = masterOrigin.X + (dx * cos - dy * sin) * scale;
            double my = masterOrigin.Y + (dx * sin + dy * cos) * scale;

            return new Point3d(mx, my, 0);
        }

        // Simple ray-cast point-in-polygon (2D) — correct even/odd test
        private static bool PointInPolygon2D(List<Point3d> poly, double x, double y)
        {
            bool inside = false;
            int n = poly.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = poly[i].X, yi = poly[i].Y;
                double xj = poly[j].X, yj = poly[j].Y;

                // Only consider edges that straddle the horizontal ray
                if ((yi > y) != (yj > y))
                {
                    // Compute the x coordinate of the intersection
                    double xint = xi + (y - yi) * (xj - xi) / (yj - yi);
                    if (x < xint) inside = !inside;
                }
            }
            return inside;
        }

        private static int FindNearIndex(List<Point3d> pts, Point3d p, double tol)
        {
            double tol2 = tol * tol;
            for (int i = 0; i < pts.Count; i++)
            {
                var q = pts[i];
                double dx = p.X - q.X, dy = p.Y - q.Y;
                if (dx * dx + dy * dy <= tol2) return i;
            }
            return -1;
        }

        private static bool HasNear(List<Point3d> pts, Point3d p, double tol)
            => FindNearIndex(pts, p, tol) >= 0;

        private static List<Point3d> DeduplicateList(List<Point3d> pts, double tol)
        {
            var outList = new List<Point3d>(pts.Count);
            foreach (var p in pts)
                if (!HasNear(outList, p, tol)) outList.Add(p);
            return outList;
        }

        // ---------------- value types ----------------

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

            public override string ToString()
                => $"SEC {Section}, TWP {Township}, RGE {Range}, MER {Meridian}";
        }

        private readonly struct Aabb2d
        {
            public Aabb2d(double minX, double minY, double maxX, double maxY)
            {
                MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY;
            }
            public double MinX { get; }
            public double MinY { get; }
            public double MaxX { get; }
            public double MaxY { get; }
        }
    }
}
