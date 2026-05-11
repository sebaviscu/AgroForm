
CREATE OR ALTER PROCEDURE [dbo].[GetLaboresByAsync]
    @IdLicencia INT,
    @IdCampania INT = NULL,
    @IdLote INT = NULL,
    @IdsLotes NVARCHAR(MAX) = NULL  -- Lista separada por comas: '1,2,3'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdCampaniaFilter INT = @IdCampania;
    DECLARE @IdLoteFilter INT = @IdLote;

    -- Tabla de IDs de lotes para filtrar
    DECLARE @LotesFilter TABLE (Id INT PRIMARY KEY);
    IF @IdsLotes IS NOT NULL
        INSERT INTO @LotesFilter
        SELECT CAST(value AS INT) FROM STRING_SPLIT(@IdsLotes, ',');

    -- ==============================
    -- S I E M B R A S
    -- ==============================
    SELECT
        s.Id,
        s.IdTipoActividad,
        ta.Nombre AS TipoActividad,
        ta.Icono AS IconoTipoActividad,
        ta.ColorIcono AS IconoColorTipoActividad,
        s.Fecha,
        s.RegistrationUser AS Responsable,
        s.RegistrationDate,
        CONCAT('Cultivo: ', c.Nombre,
               ', Superficie: ', FORMAT(s.SuperficieHa, 'N1'), ' ha',
               ', Densidad: ', FORMAT(s.DensidadSemillaKgHa, 'N1'), ' kg/ha') AS Detalle,
        s.Costo,
        s.CostoUSD,
        s.CostoARS,
        s.IdCampania,
        camp.Nombre AS Campania,
        s.Observacion,
        s.IdLote,
        l.Nombre AS Lote,
        campo.Nombre AS Campo,
        CAST(CASE WHEN s.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT) AS EsDolar,
        s.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Siembras s
    INNER JOIN Lotes l ON l.Id = s.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = s.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = s.IdTipoActividad
    INNER JOIN Cultivos c ON c.Id = s.IdCultivo
    INNER JOIN CicloCultivos cc ON cc.Id = s.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE s.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR s.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR s.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR s.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- R I E G O S
    -- ==============================
    SELECT
        r.Id,
        r.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        r.Fecha,
        r.RegistrationUser,
        r.RegistrationDate,
        CONCAT('Horas: ', FORMAT(r.HorasRiego, 'N1'), ', Volumen: ', FORMAT(r.VolumenAguaM3, 'N1'), ' m³'),
        r.Costo,
        r.CostoUSD,
        r.CostoARS,
        r.IdCampania,
        camp.Nombre,
        r.Observacion,
        r.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN r.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        r.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Riegos r
    INNER JOIN Lotes l ON l.Id = r.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = r.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = r.IdTipoActividad
    INNER JOIN CicloCultivos cc ON cc.Id = r.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE r.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR r.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR r.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR r.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- F E R T I L I Z A C I O N E S
    -- ==============================
    SELECT
        f.Id,
        f.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        f.Fecha,
        f.RegistrationUser,
        f.RegistrationDate,
        CONCAT('Nutriente: ', COALESCE(cat.Nombre, 'N/A'),
               ', Dosis: ', FORMAT(f.DosisKgHa, 'N1'), ' kg/ha'),
        f.Costo,
        f.CostoUSD,
        f.CostoARS,
        f.IdCampania,
        camp.Nombre,
        f.Observacion,
        f.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN f.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        f.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Fertilizaciones f
    INNER JOIN Lotes l ON l.Id = f.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = f.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = f.IdTipoActividad
    LEFT JOIN Catalogos cat ON cat.Id = f.IdNutriente
    INNER JOIN CicloCultivos cc ON cc.Id = f.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE f.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR f.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR f.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR f.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- P U L V E R I Z A C I O N E S
    -- ==============================
    SELECT
        p.Id,
        p.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        p.Fecha,
        p.RegistrationUser,
        p.RegistrationDate,
        CONCAT('Producto: ', COALESCE(cat.Nombre, 'N/A'),
               CASE WHEN p.VolumenLitrosHa IS NOT NULL THEN CONCAT(', Volumen: ', FORMAT(p.VolumenLitrosHa, 'N1'), ' L/ha') ELSE '' END,
               CASE WHEN p.Dosis IS NOT NULL THEN CONCAT(', Dosis: ', FORMAT(p.Dosis, 'N1')) ELSE '' END,
               CASE WHEN p.CondicionesClimaticas IS NOT NULL AND p.CondicionesClimaticas != '' THEN CONCAT(', Cond: ', p.CondicionesClimaticas) ELSE '' END),
        p.Costo,
        p.CostoUSD,
        p.CostoARS,
        p.IdCampania,
        camp.Nombre,
        p.Observacion,
        p.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN p.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        p.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Pulverizaciones p
    INNER JOIN Lotes l ON l.Id = p.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = p.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = p.IdTipoActividad
    LEFT JOIN Catalogos cat ON cat.Id = p.IdProductoAgroquimico
    INNER JOIN CicloCultivos cc ON cc.Id = p.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE p.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR p.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR p.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR p.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- M O N I T O R E O S
    -- ==============================
    SELECT
        m.Id,
        m.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        m.Fecha,
        m.RegistrationUser,
        m.RegistrationDate,
        CONCAT(catTipo.Tipo, ': ', catTipo.Nombre),
        m.Costo,
        m.CostoUSD,
        m.CostoARS,
        m.IdCampania,
        camp.Nombre,
        m.Observacion,
        m.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN m.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        m.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Monitoreos m
    INNER JOIN Lotes l ON l.Id = m.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = m.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = m.IdTipoActividad
    INNER JOIN Catalogos catTipo ON catTipo.Id = m.IdTipoMonitoreo
    INNER JOIN CicloCultivos cc ON cc.Id = m.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE m.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR m.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR m.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR m.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- C O S E C H A S
    -- ==============================
    SELECT
        cs.Id,
        cs.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        cs.Fecha,
        cs.RegistrationUser,
        cs.RegistrationDate,
        CONCAT('Cultivo: ', c.Nombre,
               CASE WHEN cs.RendimientoTonHa IS NOT NULL THEN CONCAT(', Rendimiento: ', FORMAT(cs.RendimientoTonHa, 'N1'), ' ton/ha') ELSE '' END,
               CASE WHEN cs.HumedadGrano IS NOT NULL THEN CONCAT(', Humedad: ', FORMAT(cs.HumedadGrano, 'N1'), '%') ELSE '' END,
               CASE WHEN cs.SuperficieCosechadaHa IS NOT NULL THEN CONCAT(', Sup: ', FORMAT(cs.SuperficieCosechadaHa, 'N1'), ' ha') ELSE '' END),
        cs.Costo,
        cs.CostoUSD,
        cs.CostoARS,
        cs.IdCampania,
        camp.Nombre,
        cs.Observacion,
        cs.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN cs.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        cs.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM Cosechas cs
    INNER JOIN Lotes l ON l.Id = cs.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = cs.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = cs.IdTipoActividad
    INNER JOIN Cultivos c ON c.Id = cs.IdCultivo
    INNER JOIN CicloCultivos cc ON cc.Id = cs.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE cs.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR cs.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR cs.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR cs.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- A N A L I S I S   D E   S U E L O
    -- ==============================
    SELECT
        a.Id,
        a.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        a.Fecha,
        a.RegistrationUser,
        a.RegistrationDate,
        -- Construcción condicional del detalle (como en C#)
        SUBSTRING(
            CASE WHEN a.PH IS NOT NULL THEN CONCAT('pH: ', FORMAT(a.PH, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.MateriaOrganica IS NOT NULL THEN CONCAT('MO: ', FORMAT(a.MateriaOrganica, 'N1'), '%, ') ELSE '' END +
            CASE WHEN a.Nitrogeno IS NOT NULL THEN CONCAT('N: ', FORMAT(a.Nitrogeno, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.Fosforo IS NOT NULL THEN CONCAT('P: ', FORMAT(a.Fosforo, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.Potasio IS NOT NULL THEN CONCAT('K: ', FORMAT(a.Potasio, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.ConductividadElectrica IS NOT NULL THEN CONCAT('CE: ', FORMAT(a.ConductividadElectrica, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.CIC IS NOT NULL THEN CONCAT('CIC: ', FORMAT(a.CIC, 'N1'), ', ') ELSE '' END +
            CASE WHEN a.Textura IS NOT NULL AND a.Textura != '' THEN CONCAT('Textura: ', a.Textura) ELSE '' END,
            1, LEN(
                CASE WHEN a.PH IS NOT NULL THEN CONCAT('pH: ', FORMAT(a.PH, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.MateriaOrganica IS NOT NULL THEN CONCAT('MO: ', FORMAT(a.MateriaOrganica, 'N1'), '%, ') ELSE '' END +
                CASE WHEN a.Nitrogeno IS NOT NULL THEN CONCAT('N: ', FORMAT(a.Nitrogeno, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.Fosforo IS NOT NULL THEN CONCAT('P: ', FORMAT(a.Fosforo, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.Potasio IS NOT NULL THEN CONCAT('K: ', FORMAT(a.Potasio, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.ConductividadElectrica IS NOT NULL THEN CONCAT('CE: ', FORMAT(a.ConductividadElectrica, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.CIC IS NOT NULL THEN CONCAT('CIC: ', FORMAT(a.CIC, 'N1'), ', ') ELSE '' END +
                CASE WHEN a.Textura IS NOT NULL AND a.Textura != '' THEN CONCAT('Textura: ', a.Textura) ELSE '' END
            ) - 1
        ) AS Detalle,
        a.Costo,
        a.CostoUSD,
        a.CostoARS,
        a.IdCampania,
        camp.Nombre,
        a.Observacion,
        a.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN a.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        a.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM AnalisisSuelos a
    INNER JOIN Lotes l ON l.Id = a.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = a.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = a.IdTipoActividad
    INNER JOIN CicloCultivos cc ON cc.Id = a.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE a.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR a.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR a.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR a.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- O T R A S   L A B O R E S
    -- ==============================
    SELECT
        o.Id,
        o.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        o.Fecha,
        o.RegistrationUser,
        o.RegistrationDate,
        CONCAT('Descripción: ', o.Observacion),
        o.Costo,
        o.CostoUSD,
        o.CostoARS,
        o.IdCampania,
        camp.Nombre,
        o.Observacion,
        o.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN o.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        o.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM OtrasLabores o
    INNER JOIN Lotes l ON l.Id = o.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = o.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = o.IdTipoActividad
    INNER JOIN CicloCultivos cc ON cc.Id = o.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE o.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR o.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR o.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR o.IdLote IN (SELECT Id FROM @LotesFilter))

    UNION ALL

    -- ==============================
    -- S I L O   B O L S A
    -- ==============================
    SELECT
        sb.Id,
        sb.IdTipoActividad,
        ta.Nombre,
        ta.Icono,
        ta.ColorIcono,
        sb.Fecha,
        sb.RegistrationUser,
        sb.RegistrationDate,
        CONCAT('Código: ', sb.Codigo,
               CASE WHEN sb.Longitud IS NOT NULL THEN CONCAT(', Longitud: ', FORMAT(sb.Longitud, 'N1'), ' m') ELSE '' END,
               CASE WHEN sb.CapacidadTotalTn IS NOT NULL THEN CONCAT(', Capacidad: ', FORMAT(sb.CapacidadTotalTn, 'N1'), ' tn') ELSE '' END,
               CASE WHEN sb.HumedadGrano IS NOT NULL THEN CONCAT(', Humedad: ', FORMAT(sb.HumedadGrano, 'N1'), '%') ELSE '' END),
        sb.Costo,
        sb.CostoUSD,
        sb.CostoARS,
        sb.IdCampania,
        camp.Nombre,
        sb.Observacion,
        sb.IdLote,
        l.Nombre,
        campo.Nombre,
        CAST(CASE WHEN sb.IdMoneda = 2 THEN 1 ELSE 0 END AS BIT),
        sb.IdCicloCultivo,
        CONCAT(c2.Nombre, CASE WHEN cc.FechaFin IS NULL THEN '' ELSE ' (Cerrado)' END) AS CicloCultivoNombre
    FROM SiloBolsas sb
    INNER JOIN Lotes l ON l.Id = sb.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = sb.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = sb.IdTipoActividad
    INNER JOIN CicloCultivos cc ON cc.Id = sb.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    WHERE sb.IdLicencia = @IdLicencia
      AND (@IdCampaniaFilter IS NULL OR sb.IdCampania = @IdCampaniaFilter)
      AND (@IdLoteFilter IS NULL OR sb.IdLote = @IdLoteFilter)
      AND (@IdsLotes IS NULL OR sb.IdLote IN (SELECT Id FROM @LotesFilter))

    -- ==============================
    -- O R D E N A M I E N T O
    -- ==============================
    ORDER BY RegistrationDate DESC;
END;
GO
