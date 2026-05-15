
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
               CASE WHEN s.Superficie IS NOT NULL THEN CONCAT(', Superficie: ', FORMAT(s.Superficie, 'N1'), ' ', COALESCE(um_siembra_sup.Sigla, '')) ELSE '' END,
               CASE WHEN s.SuperficieHa IS NOT NULL THEN CONCAT(' (', FORMAT(s.SuperficieHa, 'N1'), ' ha)') ELSE '' END,
               CASE WHEN s.Densidad IS NOT NULL THEN CONCAT(', Densidad: ', FORMAT(s.Densidad, 'N1'), ' ', COALESCE(um_siembra_den.Sigla, '')) ELSE '' END,
               CASE WHEN s.DensidadSemillaKgHa IS NOT NULL THEN CONCAT(' (', FORMAT(s.DensidadSemillaKgHa, 'N1'), ' kg/ha)') ELSE '' END) AS Detalle,
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        s.IdUnidadSuperficie,
        s.IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
    FROM Siembras s
    INNER JOIN Lotes l ON l.Id = s.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = s.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = s.IdTipoActividad
    INNER JOIN Cultivos c ON c.Id = s.IdCultivo
    INNER JOIN CicloCultivos cc ON cc.Id = s.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    LEFT JOIN UnidadesMedida um_siembra_sup ON s.IdUnidadSuperficie = um_siembra_sup.Id
    LEFT JOIN UnidadesMedida um_siembra_den ON s.IdUnidadDensidad = um_siembra_den.Id
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
        CONCAT('Horas: ', FORMAT(r.HorasRiego, 'N1'), ' h',
               CASE WHEN r.VolumenAgua IS NOT NULL THEN CONCAT(', Volumen: ', FORMAT(r.VolumenAgua, 'N1'), ' ', COALESCE(um_riego_vol.Sigla, '')) ELSE '' END,
               CASE WHEN r.VolumenAguaM3 IS NOT NULL THEN CONCAT(' (', FORMAT(r.VolumenAguaM3, 'N1'), ' m³)') ELSE '' END),
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        r.IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
    FROM Riegos r
    INNER JOIN Lotes l ON l.Id = r.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = r.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = r.IdTipoActividad
    INNER JOIN CicloCultivos cc ON cc.Id = r.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    LEFT JOIN UnidadesMedida um_riego_vol ON r.IdUnidadVolumenAgua = um_riego_vol.Id
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
               CASE WHEN f.Cantidad IS NOT NULL THEN CONCAT(', Cantidad: ', FORMAT(f.Cantidad, 'N1'), ' ', COALESCE(um_fert_cant.Sigla, '')) ELSE '' END,
               CASE WHEN f.Dosis IS NOT NULL THEN CONCAT(', Dosis: ', FORMAT(f.Dosis, 'N1'), ' ', COALESCE(um_fert_dosis.Sigla, '')) ELSE '' END,
               CASE WHEN f.CantidadKgHa IS NOT NULL OR f.DosisKgHa IS NOT NULL THEN
                    CONCAT(' (eq: ',
                           CASE WHEN f.CantidadKgHa IS NOT NULL THEN CONCAT(FORMAT(f.CantidadKgHa, 'N1'), ' kg/ha') ELSE '' END,
                           CASE WHEN f.CantidadKgHa IS NOT NULL AND f.DosisKgHa IS NOT NULL THEN ' | ' ELSE '' END,
                           CASE WHEN f.DosisKgHa IS NOT NULL THEN CONCAT(FORMAT(f.DosisKgHa, 'N1'), ' kg/ha') ELSE '' END,
                           ')')
               ELSE '' END),
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        f.IdUnidadCantidad,
        f.IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
    FROM Fertilizaciones f
    INNER JOIN Lotes l ON l.Id = f.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = f.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = f.IdTipoActividad
    LEFT JOIN Catalogos cat ON cat.Id = f.IdNutriente
    INNER JOIN CicloCultivos cc ON cc.Id = f.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    LEFT JOIN UnidadesMedida um_fert_cant ON f.IdUnidadCantidad = um_fert_cant.Id
    LEFT JOIN UnidadesMedida um_fert_dosis ON f.IdUnidadDosis = um_fert_dosis.Id
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
               CASE WHEN p.Volumen IS NOT NULL THEN CONCAT(', Volumen: ', FORMAT(p.Volumen, 'N1'), ' ', COALESCE(um_pulv_vol.Sigla, '')) ELSE '' END,
               CASE WHEN p.VolumenLitrosHa IS NOT NULL THEN CONCAT(' (', FORMAT(p.VolumenLitrosHa, 'N1'), ' L/ha)') ELSE '' END,
               CASE WHEN p.Dosis IS NOT NULL THEN CONCAT(', Dosis: ', FORMAT(p.Dosis, 'N1'), ' ', COALESCE(um_pulv_dosis.Sigla, '')) ELSE '' END,
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        p.IdUnidadVolumen,
        NULL AS IdUnidadCantidad,
        p.IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
    FROM Pulverizaciones p
    INNER JOIN Lotes l ON l.Id = p.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = p.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = p.IdTipoActividad
    LEFT JOIN Catalogos cat ON cat.Id = p.IdProductoAgroquimico
    INNER JOIN CicloCultivos cc ON cc.Id = p.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    LEFT JOIN UnidadesMedida um_pulv_vol ON p.IdUnidadVolumen = um_pulv_vol.Id
    LEFT JOIN UnidadesMedida um_pulv_dosis ON p.IdUnidadDosis = um_pulv_dosis.Id
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
        CONCAT('Tipo: ', catTipo.Nombre),
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
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
               CASE WHEN cs.Rendimiento IS NOT NULL THEN CONCAT(', Rendimiento: ', FORMAT(cs.Rendimiento, 'N1'), ' ', COALESCE(um_cosecha_rend.Sigla, '')) ELSE '' END,
               CASE WHEN cs.RendimientoTonHa IS NOT NULL THEN CONCAT(' (', FORMAT(cs.RendimientoTonHa, 'N1'), ' ton/ha)') ELSE '' END,
               CASE WHEN cs.HumedadGrano IS NOT NULL THEN CONCAT(', Humedad: ', FORMAT(cs.HumedadGrano, 'N1'), '%') ELSE '' END,
               CASE WHEN cs.SuperficieCosechada IS NOT NULL THEN CONCAT(', Sup: ', FORMAT(cs.SuperficieCosechada, 'N1'), ' ', COALESCE(um_cosecha_sup.Sigla, '')) ELSE '' END,
               CASE WHEN cs.SuperficieCosechadaHa IS NOT NULL THEN CONCAT(' (', FORMAT(cs.SuperficieCosechadaHa, 'N1'), ' ha)') ELSE '' END),
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        cs.IdUnidadRendimiento,
        cs.IdUnidadSuperficieCosechada
    FROM Cosechas cs
    INNER JOIN Lotes l ON l.Id = cs.IdLote
    INNER JOIN Campos campo ON campo.Id = l.IdCampo
    INNER JOIN Campanias camp ON camp.Id = cs.IdCampania
    INNER JOIN TiposActividad ta ON ta.Id = cs.IdTipoActividad
    INNER JOIN Cultivos c ON c.Id = cs.IdCultivo
    INNER JOIN CicloCultivos cc ON cc.Id = cs.IdCicloCultivo
    INNER JOIN Cultivos c2 ON c2.Id = cc.IdCultivo
    LEFT JOIN UnidadesMedida um_cosecha_rend ON cs.IdUnidadRendimiento = um_cosecha_rend.Id
    LEFT JOIN UnidadesMedida um_cosecha_sup ON cs.IdUnidadSuperficieCosechada = um_cosecha_sup.Id
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
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
    -- A C O P I O
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
        CONCAT('Código: ', a.Codigo,
               CASE WHEN a.TipoAcopio = 1 THEN ', Tipo: Silo Bolsa' ELSE '' END,
               CASE WHEN a.TipoAcopio = 2 THEN ', Tipo: Silo' ELSE '' END,
               CASE WHEN a.TipoAcopio = 3 THEN ', Tipo: Planta Externa' ELSE '' END,
               CASE WHEN a.CapacidadTotalTn IS NOT NULL THEN CONCAT(', Capacidad: ', FORMAT(a.CapacidadTotalTn, 'N1'), ' tn') ELSE '' END,
               CASE WHEN a.HumedadGrano IS NOT NULL THEN CONCAT(', Humedad: ', FORMAT(a.HumedadGrano, 'N1'), '%') ELSE '' END),
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
        c2.Nombre AS CicloCultivoNombre,
        CAST(CASE WHEN cc.FechaFin IS NULL THEN 0 ELSE 1 END AS BIT) AS EstaCerrado,
        NULL AS IdUnidadVolumenAgua,
        NULL AS IdUnidadCantidad,
        NULL AS IdUnidadDosis,
        NULL AS IdUnidadSuperficie,
        NULL AS IdUnidadDensidad,
        NULL AS IdUnidadRendimiento,
        NULL AS IdUnidadSuperficieCosechada
    FROM Acopios a
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

    -- ==============================
    -- O R D E N A M I E N T O
    -- ==============================
    ORDER BY RegistrationDate DESC;
END;
GO
