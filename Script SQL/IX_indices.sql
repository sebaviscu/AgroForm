-- ============================================
-- ÍNDICES MULTITENANT (IdLicencia) - Prioridad CRÍTICA
-- ============================================

-- Tablas principales con IdLicencia
CREATE INDEX IX_Campos_IdLicencia ON Campos (IdLicencia);
GO
CREATE INDEX IX_Campanias_IdLicencia ON Campanias (IdLicencia);
GO
CREATE INDEX IX_Lotes_IdLicencia ON Lotes (IdLicencia);
GO
CREATE INDEX IX_CicloCultivos_IdLicencia ON CicloCultivos (IdLicencia);
GO
CREATE INDEX IX_RegistrosClima_IdLicencia ON RegistrosClima (IdLicencia);
GO
CREATE INDEX IX_Gastos_IdLicencia ON Gastos (IdLicencia);
GO
CREATE INDEX IX_ReporteCierreCampania_IdLicencia ON ReporteCierreCampania (IdLicencia);
GO
CREATE INDEX IX_Usuarios_IdLicencia ON Usuarios (IdLicencia);
GO

-- Tablas de actividades (todas usan IdLicencia)
CREATE INDEX IX_Siembras_IdLicencia ON Siembras (IdLicencia);
GO
CREATE INDEX IX_Riegos_IdLicencia ON Riegos (IdLicencia);
GO
CREATE INDEX IX_Fertilizaciones_IdLicencia ON Fertilizaciones (IdLicencia);
GO
CREATE INDEX IX_Pulverizaciones_IdLicencia ON Pulverizaciones (IdLicencia);
GO
CREATE INDEX IX_Monitoreos_IdLicencia ON Monitoreos (IdLicencia);
GO
CREATE INDEX IX_AnalisisSuelos_IdLicencia ON AnalisisSuelos (IdLicencia);
GO
CREATE INDEX IX_Cosechas_IdLicencia ON Cosechas (IdLicencia);
GO
CREATE INDEX IX_OtrasLabores_IdLicencia ON OtrasLabores (IdLicencia);
GO
CREATE INDEX IX_SiloBolsas_IdLicencia ON SiloBolsas (IdLicencia);
GO

-- ============================================
-- ÍNDICES COMPUESTOS (IdLicencia + IdCampania) - Prioridad CRÍTICA
-- ============================================
-- Para el SP GetLaboresByAsync y consultas frecuentes
CREATE INDEX IX_Siembras_IdLicencia_IdCampania ON Siembras (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Riegos_IdLicencia_IdCampania ON Riegos (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Fertilizaciones_IdLicencia_IdCampania ON Fertilizaciones (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Pulverizaciones_IdLicencia_IdCampania ON Pulverizaciones (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Monitoreos_IdLicencia_IdCampania ON Monitoreos (IdLicencia, IdCampania);
GO
CREATE INDEX IX_AnalisisSuelos_IdLicencia_IdCampania ON AnalisisSuelos (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Cosechas_IdLicencia_IdCampania ON Cosechas (IdLicencia, IdCampania);
GO
CREATE INDEX IX_OtrasLabores_IdLicencia_IdCampania ON OtrasLabores (IdLicencia, IdCampania);
GO
CREATE INDEX IX_SiloBolsas_IdLicencia_IdCampania ON SiloBolsas (IdLicencia, IdCampania);
GO
CREATE INDEX IX_Lotes_IdLicencia_IdCampania ON Lotes (IdLicencia, IdCampania);
GO
CREATE INDEX IX_CicloCultivos_IdLicencia_IdCampania ON CicloCultivos (IdLicencia, IdCampania);
GO

-- ============================================
-- ÍNDICES POR LOTE (IdLicencia + IdLote) - Prioridad ALTA
-- ============================================
-- Para búsquedas por lote (muy frecuente en actividades)
CREATE INDEX IX_Siembras_IdLicencia_IdLote ON Siembras (IdLicencia, IdLote);
GO
CREATE INDEX IX_Riegos_IdLicencia_IdLote ON Riegos (IdLicencia, IdLote);
GO
CREATE INDEX IX_Fertilizaciones_IdLicencia_IdLote ON Fertilizaciones (IdLicencia, IdLote);
GO
CREATE INDEX IX_Pulverizaciones_IdLicencia_IdLote ON Pulverizaciones (IdLicencia, IdLote);
GO
CREATE INDEX IX_Monitoreos_IdLicencia_IdLote ON Monitoreos (IdLicencia, IdLote);
GO
CREATE INDEX IX_AnalisisSuelos_IdLicencia_IdLote ON AnalisisSuelos (IdLicencia, IdLote);
GO
CREATE INDEX IX_Cosechas_IdLicencia_IdLote ON Cosechas (IdLicencia, IdLote);
GO
CREATE INDEX IX_OtrasLabores_IdLicencia_IdLote ON OtrasLabores (IdLicencia, IdLote);
GO
CREATE INDEX IX_SiloBolsas_IdLicencia_IdLote ON SiloBolsas (IdLicencia, IdLote);
GO

-- ============================================
-- ÍNDICES PARA JOINS FRECUENTES - Prioridad IMPORTANTE
-- ============================================
-- Para joins con Lotes
CREATE INDEX IX_Lotes_IdCampo ON Lotes (IdCampo);
GO
CREATE INDEX IX_CicloCultivos_IdLote ON CicloCultivos (IdLote);
GO
CREATE INDEX IX_CicloCultivos_IdCultivo ON CicloCultivos (IdCultivo);
GO
CREATE INDEX IX_CicloCultivos_IdCampania ON CicloCultivos (IdCampania);
GO

-- ============================================
-- ÍNDICES PARA TABLAS DE VISIBILIDAD
-- ============================================
-- Cultivo
CREATE INDEX IX_Cultivos_IdLicencia ON Cultivos (IdLicencia);
GO
CREATE INDEX IX_LicenciasCultivos_IdLicencia ON LicenciasCultivos (IdLicencia);
GO
CREATE INDEX IX_LicenciasCultivos_IdCultivo ON LicenciasCultivos (IdCultivo);
GO

-- Catalogo
CREATE INDEX IX_Catalogos_IdLicencia ON Catalogos (IdLicencia);
GO
CREATE INDEX IX_LicenciasCatalogos_IdLicencia ON LicenciasCatalogos (IdLicencia);
GO
CREATE INDEX IX_LicenciasCatalogos_IdCatalogo ON LicenciasCatalogos (IdCatalogo);
GO

-- ============================================
-- ÍNDICES ADICIONALES IMPORTANTES
-- ============================================
-- Para búsquedas por fecha en actividades
CREATE INDEX IX_Siembras_IdLicencia_Fecha ON Siembras (IdLicencia, Fecha);
GO
CREATE INDEX IX_Riegos_IdLicencia_Fecha ON Riegos (IdLicencia, Fecha);
GO
CREATE INDEX IX_Fertilizaciones_IdLicencia_Fecha ON Fertilizaciones (IdLicencia, Fecha);
GO
CREATE INDEX IX_Pulverizaciones_IdLicencia_Fecha ON Pulverizaciones (IdLicencia, Fecha);
GO
CREATE INDEX IX_Monitoreos_IdLicencia_Fecha ON Monitoreos (IdLicencia, Fecha);
GO
CREATE INDEX IX_AnalisisSuelos_IdLicencia_Fecha ON AnalisisSuelos (IdLicencia, Fecha);
GO
CREATE INDEX IX_Cosechas_IdLicencia_Fecha ON Cosechas (IdLicencia, Fecha);
GO
CREATE INDEX IX_OtrasLabores_IdLicencia_Fecha ON OtrasLabores (IdLicencia, Fecha);
GO
CREATE INDEX IX_SiloBolsas_IdLicencia_Fecha ON SiloBolsas (IdLicencia, Fecha);
GO

