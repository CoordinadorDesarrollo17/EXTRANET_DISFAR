using System.Collections.Generic;
using Sap.Data.Hana;
using System.IO;
using System;
using System.Text;
using Capa_Entidad.Transacciones_ENT.TablasHana;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Net;
namespace Capa_Datos.Transacciones_DAO.TablasHana
{
    public class OINV_D
    {
        Utilitarios uti = new Utilitarios();
        public OINV_E BuscarComprobante(int docEntry)
        {
            StringBuilder queryBuilder = new StringBuilder($@"
    SELECT 
        t0.""DocEntry"",
        t0.""CardCode"",
        t0.""U_SYP_MDTD"" || '-' || t0.""U_SYP_MDSD"" || '-' || t0.""U_SYP_MDCD"" AS ""Correlativo"",
        t0.""DocDate"",
        t0.""DocTotal"",
        CASE
            WHEN EXISTS (
                SELECT 1 
                FROM {uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = t1.""BaseEntry""
            ) THEN 15
            ELSE 13
        END AS ""BaseType"",
        CASE
            WHEN EXISTS (
                SELECT 1 
                FROM {uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = t1.""BaseEntry""
            ) THEN (
                SELECT MAX(d.""DocEntry"")  
                FROM {uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = t1.""BaseEntry""
            )
            ELSE (
                SELECT MAX(det.""BaseEntry"")  
                FROM {uti.schemaHana}INV1 det 
                WHERE det.""DocEntry"" = t0.""DocEntry""
            )
        END AS ""BaseEntry""
    FROM {uti.schemaHana}OINV t0
    INNER JOIN {uti.schemaHana}INV1 t1 
        ON t1.""DocEntry"" = t0.""DocEntry""
    WHERE t0.""DocDate""  BETWEEN ADD_DAYS(CURRENT_DATE,-1) AND CURRENT_DATE
        AND t0.""DocEntry""={docEntry}
        AND t0.""CANCELED"" = 'N' 
        AND t0.""U_SYP_MDTD"" NOT IN ('08')
        AND t0.""DocType"" = 'I' 
        AND (
            SELECT COUNT(*)  
            FROM {uti.schemaHana}INV1 x 
            INNER JOIN {uti.schemaHana}OBTN x2 
                ON x2.""ItemCode"" = x.""ItemCode"" 
            WHERE x.""DocEntry"" = t0.""DocEntry""
        ) > 0
        AND NOT EXISTS (
            SELECT 1 
            FROM {uti.schemaHana}ORIN 
            WHERE ""U_SYP_MDTO"" || '-' || ""U_SYP_MDSO"" || '-' || ""U_SYP_MDCO"" = t0.""NumAtCard"" 
            AND ""DocTotal"" = t0.""DocTotal""
        )
");
            string queryFactura = queryBuilder.ToString();
            queryBuilder = new StringBuilder($@"
    SELECT DISTINCT 
        T1.""ItemCode"",
        COALESCE(T3.""DistNumber"", 'SIN LOTE') AS ""DistNumber"",
        T1.""LineNum"",
        T1.""WhsCode"",
        T1.""Dscription"",
        -------------------------- BaseType --------------------------
        CASE 
            -- Si existe un documento vinculado en ODLN, el BaseType será 15 (Entrega)
            WHEN EXISTS (
                SELECT 1 
                FROM {uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l 
                    ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = T1.""BaseEntry""
            ) THEN 15
            -- Si no hay entrega, el BaseType será 13 (Factura) y se tomará el BaseEntry de INV1
            ELSE 13
        END AS ""BaseType"",
        --------------------------- BaseEntry -------------------------------
        CASE 
            -- Si existe un documento en ODLN, tomar el DocEntry de la entrega como BaseEntry
            WHEN EXISTS (
                SELECT 1 
                FROM {uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l 
                    ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = T1.""BaseEntry""
            ) THEN (
                SELECT MAX(d.""DocEntry"")  
                FROM{uti.schemaHana}ODLN d
                INNER JOIN {uti.schemaHana}DLN1 l 
                    ON d.""DocEntry"" = l.""DocEntry""
                WHERE l.""DocEntry"" = T1.""BaseEntry""
            )
            -- Si no hay entrega, tomar el BaseEntry de INV1
            ELSE (
                SELECT MAX(det.""BaseEntry"")  
                FROM{uti.schemaHana}INV1 det 
                WHERE det.""DocEntry"" = T0.""DocEntry""
            )
        END AS ""BaseEntry"",
        T1.""DocEntry""
    FROM {uti.schemaHana}OINV T0  -- Factura de ventas
    INNER JOIN {uti.schemaHana}INV1 T1  -- Detalle de factura
        ON T1.""DocEntry"" = T0.""DocEntry""  
    LEFT JOIN {uti.schemaHana}DLN1 T12 
        ON T12.""DocEntry"" = T1.""BaseEntry"" 
        AND T12.""ObjType"" = T1.""BaseType"" 
        AND T12.""ItemCode"" = T1.""ItemCode"" 
        AND T12.""LineNum"" = T1.""BaseLine""
    LEFT JOIN {uti.schemaHana}OITL T2  -- Relación con la tabla OITL (Transacciones de inventario)
        ON (
            (T2.""DocEntry"" = T0.""DocEntry"" AND T2.""DocType"" = T0.""ObjType"" AND T2.""DocLine"" = T1.""LineNum"")  -- Relaciona con factura
            OR 
            (T2.""DocEntry"" = T12.""DocEntry"" AND T2.""DocType"" = T12.""ObjType"" AND T2.""DocLine"" = T12.""LineNum"")  -- Relaciona con nota de entrega
        )
        AND T2.""ItemCode"" = T1.""ItemCode"" 
        AND T2.""LocCode"" = T1.""WhsCode"" 
    LEFT JOIN {uti.schemaHana}ITL1 T21  -- Detalles de transacciones de inventario
        ON T21.""LogEntry"" = T2.""LogEntry"" 
        AND T21.""ItemCode"" = T1.""ItemCode"" 
    LEFT JOIN {uti.schemaHana}OBTN T3  -- Lotes
        ON T3.""SysNumber"" = T21.""SysNumber"" 
        AND T3.""ItemCode"" = T1.""ItemCode""  
    WHERE 
        T0.""DocEntry"" = {docEntry}
        AND T0.""U_SYP_MDTD"" NOT IN ('08')
        AND T0.""CANCELED"" = 'N'  -- Solo facturas no canceladas
        AND T0.""DocType"" = 'I'  -- Solo documentos de tipo 'I' (facturas)
        -- Excluye las facturas revertidas con una nota de crédito por el mismo monto
        AND NOT EXISTS (
            SELECT 1 
            FROM {uti.schemaHana}ORIN 
            WHERE ""U_SYP_MDTO"" || '-' || ""U_SYP_MDSO"" || '-' || ""U_SYP_MDCO"" = T0.""NumAtCard"" 
            AND ""DocTotal"" = T0.""DocTotal""
        );
");
            string queryDetFactura = queryBuilder.ToString();
            OINV_E factura = null;
            using (HanaConnection hcn = new HanaConnection(uti.cadHana))
            {
                try
                {
                    hcn.Open();
                    using (HanaCommand cmd = new HanaCommand(queryFactura, hcn))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (HanaDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                 factura = new OINV_E
                                {
                                    DocEntry = dr.IsDBNull(0) ? 0 : dr.GetInt32(0),
                                    CardCode = dr.IsDBNull(1) ? null : dr.GetString(1),
                                    Correlativo = dr.IsDBNull(2) ? null : dr.GetString(2),
                                    DocDate = dr.IsDBNull(3) ? DateTime.MinValue : dr.GetDateTime(3),
                                    DocTotal = dr.IsDBNull(4) ? 0 : dr.GetDecimal(4),
                                    BaseType = dr.IsDBNull(5) ? 0 : dr.GetInt32(5),
                                    BaseEntry= dr.IsDBNull(6) ? 0 : dr.GetInt32(6)
                                };
                            }
                        }
                    }
                    if (factura.DocEntry > 0) {
                        factura.Detalle =new List<INV1_E>();
                        using (HanaCommand cmdDetalle = new HanaCommand(queryDetFactura, hcn))
                        {
                            cmdDetalle.CommandType = CommandType.Text;
                            using (HanaDataReader drDetalle = cmdDetalle.ExecuteReader())
                            {
                                while (drDetalle.Read())
                                {
                                    INV1_E det = new INV1_E
                                    {
                                        ItemCode = drDetalle.IsDBNull(0) ? null : drDetalle.GetString(0),
                                        BatchNum = drDetalle.IsDBNull(1) ? null : drDetalle.GetString(1),
                                        LineNum = drDetalle.IsDBNull(2) ? 0 : drDetalle.GetInt32(2),
                                        WhsCode = drDetalle.IsDBNull(3) ? null : drDetalle.GetString(3),
                                        ItemName = drDetalle.IsDBNull(4) ? null : drDetalle.GetString(4),
                                        BaseType = drDetalle.IsDBNull(5) ? 0 : drDetalle.GetInt32(5),
                                        BaseEntry = drDetalle.IsDBNull(6) ? 0 : drDetalle.GetInt32(6)
                                    };
                                    factura.Detalle.Add(det);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(uti.directorioLogs + "OINV_D - BuscarComprobante.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
                }
            }
            return factura;
        }
        public List<int> ListarComprobantesAnulados(int a, int b)
        {
            List<int> lista = new List<int>();
            string query = "SELECT t0.\"DocEntry\" FROM " + uti.schemaHana + "oinv t0" +
                           " WHERE t0.\"DocEntry\" BETWEEN " + a + " AND " + b +
                           " AND t0.\"CANCELED\" = 'Y' AND t0.\"DocType\" = 'I'" +
                           " AND ((SELECT COUNT(*) FROM " + uti.schemaHana + "inv1 x" +
                           " INNER JOIN " + uti.schemaHana + "obtn x2 ON x2.\"ItemCode\" = x.\"ItemCode\"" +
                           " WHERE x.\"DocEntry\" = t0.\"DocEntry\") > 0)" +
                           " ORDER BY t0.\"DocEntry\"";
            using (HanaConnection hcn = new HanaConnection(uti.cadHana))
            {
                try
                {
                    hcn.Open();
                    using (HanaCommand cmd = new HanaCommand(query, hcn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        using (HanaDataReader hdr = cmd.ExecuteReader())
                        {
                            while (hdr.Read())
                            {
                                if (!hdr.IsDBNull(0))
                                {
                                    int docEntry = hdr.GetInt32(0);
                                    lista.Add(docEntry);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Registro de errores (opcional)
                    File.AppendAllText(uti.directorioLogs + "OINV_D - listarComprobantesAnulados.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
                }
            }
            return lista;
        }
        public int ImportarComprobante(OINV_E obj)
        {
            string queryFactura = "insert into dbo.Factura values(@DocEntry,@CardCode,@Correlativo,@DocDate,@DocTotal,@BaseType,@BaseEntry);";
            string queryDetFactura = "insert into dbo.DetFactura values(@ItemCode,@BatchNum,@LineNum,@WhsCode,@ItemName,@BaseType,@BaseEntry);";
            SqlConnection cn = new SqlConnection(uti.cadSql);
            try
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();
                try
                {
                        if (obj.Detalle.Count > 0)
                        {
                            SqlCommand cmd = new SqlCommand(queryFactura, cn);
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@DocEntry", obj.DocEntry);
                            cmd.Parameters.AddWithValue("@CardCode", obj.CardCode);
                            cmd.Parameters.AddWithValue("@Correlativo", obj.Correlativo);
                            cmd.Parameters.AddWithValue("@DocDate", obj.DocDate);
                            cmd.Parameters.AddWithValue("@DocTotal", obj.DocTotal);
                            cmd.Parameters.AddWithValue("@BaseType", obj.BaseType);
                            cmd.Parameters.AddWithValue("@BaseEntry", obj.BaseEntry);
                            cmd.ExecuteNonQuery();
                            foreach (INV1_E det in obj.Detalle)
                            {
                                SqlCommand cmd2 = new SqlCommand(queryDetFactura, cn);
                                cmd2.Transaction = tran;
                                cmd2.CommandType = CommandType.Text;
                                cmd2.Parameters.AddWithValue("@ItemCode", det.ItemCode);
                                cmd2.Parameters.AddWithValue("@BatchNum", det.BatchNum);
                                cmd2.Parameters.AddWithValue("@LineNum", det.LineNum);
                                cmd2.Parameters.AddWithValue("@WhsCode", det.WhsCode);
                                cmd2.Parameters.AddWithValue("@ItemName", det.ItemName);
                                cmd2.Parameters.AddWithValue("@BaseType", det.BaseType);
                                cmd2.Parameters.AddWithValue("@BaseEntry", det.BaseEntry);
                                cmd2.ExecuteNonQuery();
                            }
                    }
                        tran.Commit();
                        cn.Close();
                }
                catch (Exception ex)
                { 
                    File.AppendAllText(uti.directorioLogs + "OINV_D - ImportarComprobantes.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
                    return 0;
                }
            }
            catch (Exception e2) { cn.Close(); throw new Exception("Error en conexion: " + e2.Message); }
            return 1;
        }
        public List <int> ListarDocEntryComprobantes(string cardCode)
        {
            StringBuilder queryBuilder = new StringBuilder($@"
    SELECT DISTINCT
        t0.""DocEntry""
    FROM {uti.schemaHana}OINV t0
    INNER JOIN {uti.schemaHana}INV1 t1 
        ON t1.""DocEntry"" = t0.""DocEntry""
    WHERE t0.""DocDate""  = CURRENT_DATE
        AND t0.""CardCode""='{cardCode}'
        AND t0.""CANCELED"" = 'N' 
        AND t0.""U_SYP_MDTD"" NOT IN ('08')
        AND t0.""DocType"" = 'I' 
        AND (
            SELECT COUNT(*)  
            FROM {uti.schemaHana}INV1 x 
            INNER JOIN {uti.schemaHana}OBTN x2 
                ON x2.""ItemCode"" = x.""ItemCode"" 
            WHERE x.""DocEntry"" = t0.""DocEntry""
        ) > 0
        AND NOT EXISTS (
            SELECT 1 
            FROM {uti.schemaHana}ORIN 
            WHERE ""U_SYP_MDTO"" || '-' || ""U_SYP_MDSO"" || '-' || ""U_SYP_MDCO"" = t0.""NumAtCard"" 
            AND ""DocTotal"" = t0.""DocTotal""
        )
");
            string query = queryBuilder.ToString();
            List<int> lista = new List<int>();
            using (HanaConnection hcn = new HanaConnection(uti.cadHana))
            {
                try
                {
                    hcn.Open();
                    using (HanaCommand cmd = new HanaCommand(query, hcn))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (HanaDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int docEntry = dr.IsDBNull(0) ? 0 : dr.GetInt32(0);
                                if (docEntry > 0)
                                    lista.Add(docEntry);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(uti.directorioLogs + "OINV_D - ListarComprobante.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
                }
            }
            return lista;
        }
        //LINEA 74 
        //queryBuilder.AppendLine("SELECT t0.\"DocEntry\", t0.\"CardCode\", t0.\"Series\", t0.\"U_SYP_MDTD\", t0.\"U_SYP_MDSD\", t0.\"U_SYP_MDCD\", t0.\"DocDate\", t0.\"DocTotal\"")
        //            .AppendLine("FROM " + uti.schemaHana + "oinv t0")
        //            .AppendLine("WHERE 1=1 ")
        //            .AppendLine(" AND t0.\"CANCELED\" = 'N' AND t0.\"DocType\" = 'I'")
        //            .AppendLine(" AND (SELECT COUNT(*) FROM " + uti.schemaHana + "inv1 x")
        //            .AppendLine("INNER JOIN " + uti.schemaHana + "obtn x2 ON x2.\"ItemCode\" = x.\"ItemCode\"")
        //            .AppendLine("WHERE x.\"DocEntry\" = t0.\"DocEntry\") > 0  ")
        //            .AppendLine($"AND t0.\"CardCode\"='{cardCode}'")
        //            .AppendLine("ORDER BY t0.\"DocEntry\"");
    }
}
