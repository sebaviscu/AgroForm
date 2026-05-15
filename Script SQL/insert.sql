

USE AgroForm;


INSERT INTO Usuarios 
(IdLicencia, Nombre, Email, Rol, Activo, PasswordHash, PasswordSalt, EmailConfirmed, RegistrationDate, SuperAdmin, RegistrationUser)
VALUES
(
    NULL, 
    'Administrador', 
    'admin@agroform.com', 
    2, -- Rol administrador, según tu enum
    1,
    '0Zh5ZmURh/ssi3KeLS1HZqYDXqf9RVLHEVmMTqzkDkmYFwS186VLfv2sUJBpEQ8gWOf3xSTb4rSEJk4iVFWsng==',
    0x77168C588E15B95C907FCFB6E51EB6C481A23D79C17167EA3ABAE95DCA1DAFB5D1EE681A763C4CD7190582F853FA7F842C006209F83882B5CF5AE5308E850B4E696337E6000BC1A4EBB8736EE4B4EE416DC140F1B5729E099EFD0D1E4ACB63DAC809575FA861EF36308BA33F392A95750823FBFB94A12FACE79E8DC6D1344701
    ,1,
    GETDATE(),
	1,
    'System'
);

INSERT INTO Monedas (Codigo, Nombre, Simbolo, TipoCambioReferencia, RegistrationDate, RegistrationUser) VALUES
('ARS', 'Peso Argentino', '$', 1, GETDATE(), 'System'),
('USD', 'Oficial', 'US$', 1500, GETDATE(), 'System');



INSERT INTO TiposActividad (Nombre, Icono, ColorIcono)
VALUES
('Analisis de suelo', 'ph-flask', '#607D8B'),
('Siembra', 'ph-leaf', '#4CAF50'),
('Pulverizacion', 'ph-spray-bottle', '#FF9800'),
('Fertilizado', 'ph-test-tube', '#9C27B0'),
('Riego', 'ph-drop', '#03A9F4'),
('Monitoreo', 'ph-magnifying-glass', '#795548'),
('Cosecha', 'ph-grains', '#FFC107'),
('Otras labores', 'ph-wrench', '#9E9E9E'),
('Acopio', 'ph-warehouse', '#8B5E3C');


-- Insertar Cultivos (sin IDs explícitos y sin Descripcion)
INSERT INTO Cultivos (Nombre, Orden, Activo, Color, RegistrationDate) VALUES
('SOJA', 1, 1, '#4CAF50', GETDATE()),
('MAÍZ', 2, 1, '#FF9800', GETDATE()),
('TRIGO', 3, 1, '#2196F3', GETDATE()),
('SORGO', 4, 1, '#9C27B0', GETDATE()),
('GIRASOL', 5, 1, '#FFC107', GETDATE()),
('MIJO', 6, 0, '#795548', GETDATE()),
('LINO', 7, 0, '#00BCD4', GETDATE()),
('AVENA', 8, 0, '#795548', GETDATE()),
('ARROZ', 9, 0, '#607D8B', GETDATE()),
('CEBADA', 10, 0, '#00BCD4', GETDATE()),
('GARBANZO', 11, 0, '#8BC34A', GETDATE()),
('COLZA', 12, 0, '#607D8B', GETDATE()),
('ALGODÓN', 13, 0, '#E91E63', GETDATE()),
('CHÍA', 14, 0, '#4CAF50', GETDATE()),
('MANÍ', 15, 0, '#FF9800', GETDATE()),
('POROTO', 16, 0, '#8BC34A', GETDATE()),
('ARVEJA', 17, 0, '#8BC34A', GETDATE()),
('LENTEJA', 18, 0, '#8BC34A', GETDATE()),
('QUINOA', 19, 0, '#4CAF50', GETDATE()),
('ALPISTE', 20, 0, '#8BC34A', GETDATE()),
('SESAMO', 21, 0, '#FFC107', GETDATE()),
('CÁRTAMO', 22, 0, '#FFC107', GETDATE()),
('CORIANDRO', 23, 0, '#4CAF50', GETDATE()),
('CARINATA', 24, 0, '#607D8B', GETDATE()),
('CAMELINA', 25, 0, '#607D8B', GETDATE()),
('CENTENO', 26, 0, '#E91E63', GETDATE());


-- Insertar Estados Fenológicos
INSERT INTO EstadosFenologicos (IdCultivo, Codigo, Nombre, Descripcion, Activo, RegistrationDate) VALUES
-- SOJA (IdCultivo = 1)
(1, 'VE', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(1, 'VC', 'Cotiledón', 'Cotiledones completamente expandidos.', 1, GETDATE()),
(1, 'V1', 'Primer nudo', 'Primer nudo con hoja unifoliada.', 1, GETDATE()),
(1, 'V2', 'Segundo nudo', 'Segundo nudo con hoja trifoliada.', 1, GETDATE()),
(1, 'V3', 'Tercer nudo', 'Tercer nudo con hoja trifoliada.', 1, GETDATE()),
(1, 'R1', 'Inicio de floración', 'Primera flor abierta en nudo principal.', 1, GETDATE()),
(1, 'R2', 'Floración plena', 'Floración en nudo principal y ramas.', 1, GETDATE()),
(1, 'R3', 'Inicio de formación de vainas', 'Vainas de 5 mm en nudos superiores.', 1, GETDATE()),
(1, 'R4', 'Vainas completamente desarrolladas', 'Vainas de tamaño máximo en nudos superiores.', 1, GETDATE()),
(1, 'R5', 'Inicio de llenado de granos', 'Granos comenzando a desarrollarse en vainas.', 1, GETDATE()),
(1, 'R6', 'Madurez fisiológica', 'Granos completamente desarrollados, vainas amarillas.', 1, GETDATE()),
(1, 'R7', 'Madurez de cosecha', 'Plantas secas, listas para cosecha.', 1, GETDATE()),

-- MAÍZ (IdCultivo = 2)
(2, 'VE', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(2, 'V1', 'Primera hoja', 'Primera hoja completamente expandida.', 1, GETDATE()),
(2, 'V3', 'Tercera hoja', 'Tercera hoja completamente expandida.', 1, GETDATE()),
(2, 'V6', 'Sexta hoja', 'Sexta hoja completamente expandida.', 1, GETDATE()),
(2, 'V10', 'Décima hoja', 'Décima hoja completamente expandida.', 1, GETDATE()),
(2, 'VT', 'Espigazón', 'Espiga completamente visible.', 1, GETDATE()),
(2, 'R1', 'Embuchado', 'Estigmas (barbas) visibles.', 1, GETDATE()),
(2, 'R2', 'Amacollamiento', 'Granos en estado lechoso.', 1, GETDATE()),
(2, 'R3', 'Granos pastosos', 'Granos con consistencia pastosa.', 1, GETDATE()),
(2, 'R4', 'Granos dentados', 'Granos con textura dentada.', 1, GETDATE()),
(2, 'R5', 'Granos duros', 'Granos con textura dura.', 1, GETDATE()),
(2, 'R6', 'Madurez fisiológica', 'Granos con línea de abscisión negra.', 1, GETDATE()),

-- TRIGO (IdCultivo = 3)
(3, 'Z10', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(3, 'Z13', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(3, 'Z21', 'Inicio de macollaje', 'Primer macollo visible.', 1, GETDATE()),
(3, 'Z25', 'Cinco macollos', 'Cinco macollos formados.', 1, GETDATE()),
(3, 'Z30', 'Fin de macollaje', 'Fin del período de macollaje.', 1, GETDATE()),
(3, 'Z31', 'Primer nudo', 'Primer nudo visible en tallo principal.', 1, GETDATE()),
(3, 'Z39', 'Ligula de la hoja bandera', 'Ligula de la hoja bandera visible.', 1, GETDATE()),
(3, 'Z55', 'Media espiga', 'Espiga mitad de su tamaño final.', 1, GETDATE()),
(3, 'Z65', 'Floración completa', 'Floración completa en la espiga.', 1, GETDATE()),
(3, 'Z71', 'Granos acuosos', 'Granos en estado acuoso.', 1, GETDATE()),
(3, 'Z85', 'Madurez de cosecha', 'Granos duros, listos para cosecha.', 1, GETDATE()),
(3, 'Z92', 'Madurez completa', 'Granos completamente secos.', 1, GETDATE()),

-- SORGO (IdCultivo = 4)
(4, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(4, 'V3', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(4, 'V6', 'Seis hojas', 'Seis hojas completamente expandidas.', 1, GETDATE()),
(4, 'V9', 'Nueve hojas', 'Nueve hojas completamente expandidas.', 1, GETDATE()),
(4, 'B', 'Botonamiento', 'Inicio de formación de panoja.', 1, GETDATE()),
(4, 'H', 'Espigazón', 'Panoja completamente emergida.', 1, GETDATE()),
(4, 'F', 'Floración', 'Floración en la panoja.', 1, GETDATE()),
(4, 'S', 'Granos lechosos', 'Granos en estado lechoso.', 1, GETDATE()),
(4, 'D', 'Granos pastosos', 'Granos en estado pastoso.', 1, GETDATE()),
(4, 'M', 'Madurez fisiológica', 'Granos duros, listos para cosecha.', 1, GETDATE()),

-- GIRASOL (IdCultivo = 5)
(5, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(5, 'V2', 'Dos pares de hojas', 'Dos pares de hojas verdaderas.', 1, GETDATE()),
(5, 'V4', 'Cuatro pares de hojas', 'Cuatro pares de hojas verdaderas.', 1, GETDATE()),
(5, 'V6', 'Seis pares de hojas', 'Seis pares de hojas verdaderas.', 1, GETDATE()),
(5, 'V8', 'Ocho pares de hojas', 'Ocho pares de hojas verdaderas.', 1, GETDATE()),
(5, 'R1', 'Botonamiento', 'Botón floral visible.', 1, GETDATE()),
(5, 'R2', 'Inicio de floración', 'Primeras flores del capítulo abiertas.', 1, GETDATE()),
(5, 'R5', 'Floración completa', 'Floración completa del capítulo.', 1, GETDATE()),
(5, 'R6', 'Fin de floración', 'Fin del período de floración.', 1, GETDATE()),
(5, 'R7', 'Llenado de granos', 'Granos en desarrollo.', 1, GETDATE()),
(5, 'R8', 'Madurez fisiológica', 'Parte trasera del capítulo amarilla.', 1, GETDATE()),
(5, 'R9', 'Madurez de cosecha', 'Capítulo seco, granos duros.', 1, GETDATE()),

-- MIJO (IdCultivo = 6)
(6, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(6, 'V3', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(6, 'V6', 'Seis hojas', 'Seis hojas completamente expandidas.', 1, GETDATE()),
(6, 'B', 'Botonamiento', 'Inicio de formación de panoja.', 1, GETDATE()),
(6, 'H', 'Espigazón', 'Panoja completamente emergida.', 1, GETDATE()),
(6, 'F', 'Floración', 'Floración en la panoja.', 1, GETDATE()),
(6, 'M', 'Madurez', 'Granos duros, listos para cosecha.', 1, GETDATE()),

-- LINO (IdCultivo = 7)
(7, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(7, 'V4', 'Cuatro pares de hojas', 'Cuatro pares de hojas verdaderas.', 1, GETDATE()),
(7, 'V8', 'Ocho pares de hojas', 'Ocho pares de hojas verdaderas.', 1, GETDATE()),
(7, 'B', 'Botonamiento', 'Botones florales visibles.', 1, GETDATE()),
(7, 'F', 'Floración', 'Flores completamente abiertas.', 1, GETDATE()),
(7, 'G', 'Formación de cápsulas', 'Cápsulas en desarrollo.', 1, GETDATE()),
(7, 'M', 'Madurez', 'Cápsulas secas, semillas maduras.', 1, GETDATE()),

-- AVENA (IdCultivo = 8)
(8, 'Z10', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(8, 'Z13', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(8, 'Z21', 'Inicio de macollaje', 'Primer macollo visible.', 1, GETDATE()),
(8, 'Z31', 'Primer nudo', 'Primer nudo visible en tallo principal.', 1, GETDATE()),
(8, 'Z39', 'Ligula de la hoja bandera', 'Ligula de la hoja bandera visible.', 1, GETDATE()),
(8, 'Z55', 'Media espiga', 'Espiga mitad de su tamaño final.', 1, GETDATE()),
(8, 'Z65', 'Floración completa', 'Floración completa en la espiga.', 1, GETDATE()),
(8, 'Z71', 'Granos acuosos', 'Granos en estado acuoso.', 1, GETDATE()),
(8, 'Z85', 'Madurez de cosecha', 'Granos duros, listos para cosecha.', 1, GETDATE()),

-- ARROZ (IdCultivo = 9)
(9, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(9, 'V3', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(9, 'V6', 'Seis hojas', 'Seis hojas completamente expandidas.', 1, GETDATE()),
(9, 'V9', 'Nueve hojas', 'Nueve hojas completamente expandidas.', 1, GETDATE()),
(9, 'PI', 'Iniciación panicular', 'Inicio de formación de panícula.', 1, GETDATE()),
(9, 'B', 'Botonamiento', 'Botón panicular visible.', 1, GETDATE()),
(9, 'H', 'Espigazón', 'Panícula completamente emergida.', 1, GETDATE()),
(9, 'F', 'Floración', 'Floración en la panícula.', 1, GETDATE()),
(9, 'M', 'Madurez', 'Granos duros, listos para cosecha.', 1, GETDATE()),

-- CEBADA (IdCultivo = 10)
(10, 'Z10', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(10, 'Z13', 'Tres hojas', 'Tres hojas completamente expandidas.', 1, GETDATE()),
(10, 'Z21', 'Inicio de macollaje', 'Primer macollo visible.', 1, GETDATE()),
(10, 'Z31', 'Primer nudo', 'Primer nudo visible en tallo principal.', 1, GETDATE()),
(10, 'Z39', 'Ligula de la hoja bandera', 'Ligula de la hoja bandera visible.', 1, GETDATE()),
(10, 'Z55', 'Media espiga', 'Espiga mitad de su tamaño final.', 1, GETDATE()),
(10, 'Z65', 'Floración completa', 'Floración completa en la espiga.', 1, GETDATE()),
(10, 'Z71', 'Granos acuosos', 'Granos en estado acuoso.', 1, GETDATE()),
(10, 'Z85', 'Madurez de cosecha', 'Granos duros, listos para cosecha.', 1, GETDATE()),

-- GARBANZO (IdCultivo = 11)
(11, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(11, 'V2', 'Desarrollo vegetativo', 'Inicio del crecimiento de ramas.', 1, GETDATE()),
(11, 'R1', 'Inicio de floración', 'Primera flor abierta.', 1, GETDATE()),
(11, 'R3', 'Formación de vaina', 'Aparición de las primeras vainas.', 1, GETDATE()),
(11, 'R6', 'Madurez fisiológica', 'Semillas completamente desarrolladas.', 1, GETDATE()),

-- COLZA (IdCultivo = 12)
(12, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(12, 'R1', 'Roseta', 'Formación de hojas basales.', 1, GETDATE()),
(12, 'B', 'Botonamiento', 'Inicio de elongación del tallo.', 1, GETDATE()),
(12, 'F', 'Floración', 'Flores abiertas.', 1, GETDATE()),
(12, 'M', 'Madurez', 'Síliquas secas y semillas duras.', 1, GETDATE()),

-- ALGODÓN (IdCultivo = 13)
(13, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(13, 'V6', 'Desarrollo vegetativo', 'Planta con ramas fructíferas.', 1, GETDATE()),
(13, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(13, 'B', 'Formación de cápsulas', 'Cápsulas visibles en las ramas.', 1, GETDATE()),
(13, 'M', 'Madurez', 'Cápsulas abiertas con fibra visible.', 1, GETDATE()),

-- CHÍA (IdCultivo = 14)
(14, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(14, 'V4', 'Desarrollo vegetativo', 'Crecimiento de hojas y ramas.', 1, GETDATE()),
(14, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(14, 'M', 'Madurez', 'Semillas oscuras y duras.', 1, GETDATE()),

-- MANÍ (IdCultivo = 15)
(15, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(15, 'V3', 'Desarrollo vegetativo', 'Formación de ramas.', 1, GETDATE()),
(15, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(15, 'G', 'Clavado', 'Inicio de penetración de ginecóforos al suelo.', 1, GETDATE()),
(15, 'M', 'Madurez', 'Vainas secas y semillas maduras.', 1, GETDATE()),

-- POROTO (IdCultivo = 16)
(16, 'E', 'Emergencia', 'Aparición de los cotiledones.', 1, GETDATE()),
(16, 'V2', 'Desarrollo vegetativo', 'Desarrollo de hojas y ramas.', 1, GETDATE()),
(16, 'R1', 'Floración', 'Primera flor abierta.', 1, GETDATE()),
(16, 'R3', 'Llenado de grano', 'Desarrollo de las vainas.', 1, GETDATE()),
(16, 'R6', 'Madurez', 'Vainas secas, semillas duras.', 1, GETDATE()),

-- ARVEJA (IdCultivo = 17)
(17, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(17, 'V4', 'Desarrollo vegetativo', 'Desarrollo de tallos y hojas.', 1, GETDATE()),
(17, 'R1', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(17, 'R3', 'Formación de vaina', 'Primeras vainas visibles.', 1, GETDATE()),
(17, 'R6', 'Madurez fisiológica', 'Semillas maduras.', 1, GETDATE()),

-- LENTEJA (IdCultivo = 18)
(18, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(18, 'V2', 'Desarrollo vegetativo', 'Inicio de ramificación.', 1, GETDATE()),
(18, 'R1', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(18, 'R3', 'Formación de vaina', 'Primeras vainas formadas.', 1, GETDATE()),
(18, 'R6', 'Madurez', 'Vainas secas y semillas maduras.', 1, GETDATE()),

-- QUINOA (IdCultivo = 19)
(19, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(19, 'V4', 'Desarrollo vegetativo', 'Crecimiento de hojas y tallos.', 1, GETDATE()),
(19, 'B', 'Botonamiento', 'Inicio de diferenciación floral.', 1, GETDATE()),
(19, 'F', 'Floración', 'Apertura de flores.', 1, GETDATE()),
(19, 'M', 'Madurez', 'Granos duros y secos.', 1, GETDATE()),

-- ALPISTE (IdCultivo = 20)
(20, 'Z10', 'Emergencia', 'Plántula emerge.', 1, GETDATE()),
(20, 'Z21', 'Macollaje', 'Inicio del macollaje.', 1, GETDATE()),
(20, 'Z31', 'Encañado', 'Primer nudo visible.', 1, GETDATE()),
(20, 'Z51', 'Espigazón', 'Inicio de espigazón.', 1, GETDATE()),
(20, 'Z85', 'Madurez', 'Grano duro y seco.', 1, GETDATE()),

-- SESAMO (IdCultivo = 21)
(21, 'E', 'Emergencia', 'Plántula emerge.', 1, GETDATE()),
(21, 'V4', 'Desarrollo vegetativo', 'Desarrollo de hojas y ramas.', 1, GETDATE()),
(21, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(21, 'M', 'Madurez', 'Cápsulas secas y semillas maduras.', 1, GETDATE()),

-- CÁRTAMO (IdCultivo = 22)
(22, 'E', 'Emergencia', 'Plántula emerge.', 1, GETDATE()),
(22, 'R1', 'Roseta', 'Formación de hojas basales.', 1, GETDATE()),
(22, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(22, 'M', 'Madurez', 'Cabezas secas, semillas duras.', 1, GETDATE()),

-- CORIANDRO (IdCultivo = 23)
(23, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(23, 'V3', 'Desarrollo vegetativo', 'Desarrollo de hojas compuestas.', 1, GETDATE()),
(23, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(23, 'M', 'Madurez', 'Umbelas secas y semillas duras.', 1, GETDATE()),

-- CARINATA (IdCultivo = 24)
(24, 'E', 'Emergencia', 'Plántula emerge.', 1, GETDATE()),
(24, 'R1', 'Roseta', 'Formación de hojas basales.', 1, GETDATE()),
(24, 'B', 'Botonamiento', 'Inicio de elongación del tallo.', 1, GETDATE()),
(24, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(24, 'M', 'Madurez', 'Síliquas secas.', 1, GETDATE()),

-- CAMELINA (IdCultivo = 25)
(25, 'E', 'Emergencia', 'Plántula emerge del suelo.', 1, GETDATE()),
(25, 'V6', 'Desarrollo vegetativo', 'Crecimiento de hojas.', 1, GETDATE()),
(25, 'F', 'Floración', 'Inicio de floración.', 1, GETDATE()),
(25, 'M', 'Madurez', 'Frutos secos, semillas maduras.', 1, GETDATE()),

-- CENTENO (IdCultivo = 26)
(26, 'Z10', 'Emergencia', 'Plántula emerge.', 1, GETDATE()),
(26, 'Z21', 'Macollaje', 'Inicio del macollaje.', 1, GETDATE()),
(26, 'Z31', 'Encañado', 'Primer nudo visible.', 1, GETDATE()),
(26, 'Z51', 'Espigazón', 'Inicio de espigazón.', 1, GETDATE()),
(26, 'Z85', 'Madurez', 'Grano duro.', 1, GETDATE());


INSERT INTO Catalogos (Tipo, Nombre, Descripcion, Activo, RegistrationDate, RegistrationUser)
VALUES

-- Plagas
(10, 'Oruga Cogollera', 'Spodoptera frugiperda, ataca maíz y sorgo', 1, GETDATE(), 'admin'),
(10, 'Chinche verde', 'Nezara viridula, afecta soja y maíz', 1, GETDATE(), 'admin'),
(10, 'Trips', 'Pequeños insectos que succionan savia', 1, GETDATE(), 'admin'),
(10, 'Barrenador del tallo', 'Ataca maíz y caña de azúcar', 1, GETDATE(), 'admin'),
(10, 'Gusano blanco', 'Larva del escarabajo que daña raíces', 1, GETDATE(), 'admin'),
(10, 'Minador de hojas', 'Crea galerías dentro de las hojas', 1, GETDATE(), 'admin'),
(10, 'Pulgón amarillo del sorgo', 'Melanaphis sacchari, afecta hojas', 1, GETDATE(), 'admin'),
(10, 'Mosca blanca', 'Transmite virosis en cultivos hortícolas', 1, GETDATE(), 'admin'),
(10, 'Pulgón del sorgo', 'Insecto que afecta hojas y tallos', 1, GETDATE(), 'admin'),

-- Malezas
(11, 'Amaranthus palmeri', 'Maleza resistente a herbicidas', 1, GETDATE(), 'admin'),
(11, 'Conyza bonariensis', 'Rama negra, resistente a herbicidas', 1, GETDATE(), 'admin'),
(11, 'Sorghum halepense', 'Sorgo de Alepo, perenne invasiva', 1, GETDATE(), 'admin'),
(11, 'Echinochloa colona', 'Pasto cuaresma, frecuente en arroz', 1, GETDATE(), 'admin'),
(11, 'Lolium multiflorum', 'Raigrás anual resistente', 1, GETDATE(), 'admin'),
(11, 'Digitaria sanguinalis', 'Pasto cuaresma, de ciclo anual', 1, GETDATE(), 'admin'),
(11, 'Bidens pilosa', 'Picapica, maleza de hoja ancha', 1, GETDATE(), 'admin'),
(11, 'Chenopodium album', 'Quinoa silvestre, común en barbechos', 1, GETDATE(), 'admin'),

-- Enfermedades
(12, 'Roya del trigo', 'Puccinia triticina, causa manchas anaranjadas', 1, GETDATE(), 'admin'),
(12, 'Tizón del maíz', 'Exserohilum turcicum, afecta hojas', 1, GETDATE(), 'admin'),
(12, 'Mancha ojo de rana', 'Cercospora sojina en soja', 1, GETDATE(), 'admin'),
(12, 'Cancro del tallo', 'Diaporthe phaseolorum, afecta tallos de soja', 1, GETDATE(), 'admin'),
(12, 'Podredumbre gris', 'Botrytis cinerea en hortalizas', 1, GETDATE(), 'admin'),
(12, 'Mildiu', 'Plasmopara viticola, afecta hojas de vid', 1, GETDATE(), 'admin'),
(12, 'Fusariosis', 'Fusarium spp., afecta raíces y tallos', 1, GETDATE(), 'admin'),
(12, 'Antracnosis', 'Colletotrichum spp., provoca lesiones en hojas', 1, GETDATE(), 'admin'),

-- Tipo de fertilizante
(20, 'Granulado', 'Fertilizante sólido granulado', 1, GETDATE(), 'admin'),
(20, 'Líquido', 'Fertilizante soluble en agua', 1, GETDATE(), 'admin'),
(20, 'Foliar', 'Aplicación directa sobre hojas', 1, GETDATE(), 'admin'),
(20, 'Orgánico', 'Proveniente de compost, estiércol o residuos', 1, GETDATE(), 'admin'),
(20, 'Químico', 'Fertilizante sintético con nutrientes concentrados', 1, GETDATE(), 'admin'),
(20, 'Control liberación lenta', 'Libera nutrientes progresivamente', 1, GETDATE(), 'admin'),
(20, 'Microgranulado', 'Fertilizante fino de rápida disolución', 1, GETDATE(), 'admin'),

-- Nutrientes
(21, 'Nitrógeno (N)', 'Estimula crecimiento vegetativo', 1, GETDATE(), 'admin'),
(21, 'Fósforo (P)', 'Favorece desarrollo radicular', 1, GETDATE(), 'admin'),
(21, 'Potasio (K)', 'Aumenta resistencia a enfermedades', 1, GETDATE(), 'admin'),
(21, 'Azufre (S)', 'Participa en síntesis de proteínas', 1, GETDATE(), 'admin'),
(21, 'Calcio (Ca)', 'Fortalece estructura celular', 1, GETDATE(), 'admin'),
(21, 'Magnesio (Mg)', 'Componente central de la clorofila', 1, GETDATE(), 'admin'),
(21, 'Zinc (Zn)', 'Interviene en formación de hormonas de crecimiento', 1, GETDATE(), 'admin'),
(21, 'Hierro (Fe)', 'Necesario para la fotosíntesis', 1, GETDATE(), 'admin'),
(21, 'Boro (B)', 'Favorece floración y fructificación', 1, GETDATE(), 'admin'),

-- Productos agroquímicos
(22, 'Glifosato', 'Herbicida sistémico no selectivo', 1, GETDATE(), 'admin'),
(22, 'Atrazina', 'Herbicida selectivo para maíz', 1, GETDATE(), 'admin'),
(22, '2,4-D', 'Herbicida hormonal postemergente', 1, GETDATE(), 'admin'),
(22, 'Clorpirifos', 'Insecticida de amplio espectro', 1, GETDATE(), 'admin'),
(22, 'Lambda cihalotrina', 'Insecticida piretroide', 1, GETDATE(), 'admin'),
(22, 'Azoxistrobina', 'Fungicida preventivo', 1, GETDATE(), 'admin'),
(22, 'Tebuconazole', 'Fungicida triazol', 1, GETDATE(), 'admin'),
(22, 'Imidacloprid', 'Insecticida neonicotinoide', 1, GETDATE(), 'admin'),
(22, 'Paraquat', 'Herbicida de contacto', 1, GETDATE(), 'admin'),


(30, 'Siembra directa', 'Método sin laboreo previo del suelo', 1, GETDATE(), 'admin'),
(30, 'Siembra convencional', 'Labranza previa antes de la siembra', 1, GETDATE(), 'admin'),

(31, 'Riego por goteo', 'Ahorra agua y nutrientes', 1, GETDATE(), 'admin'),
(31, 'Riego por aspersión', 'Método de riego mediante rociadores', 1, GETDATE(), 'admin'),
(32, 'Aplicación terrestre', 'Uso de maquinaria agrícola terrestre', 1, GETDATE(), 'admin'),
(32, 'Aplicación aérea', 'Uso de avión o dron para aplicar insumos', 1, GETDATE(), 'admin'),


(40, 'Tractor', 'Tractor', 1, GETDATE(), 'admin'),
(40, 'Pulverizadora', 'Pulverizadora', 1, GETDATE(), 'admin'),
(40, 'Sembradora', 'Sembradora', 1, GETDATE(), 'admin'),
(40, 'Cosechadora', 'Cosechadora', 1, GETDATE(), 'admin'),
(40, 'Rastra de discos', 'Rastra de arrastre para laboreo', 1, GETDATE(), 'admin'),


(41, 'Pozo profundo', 'Fuente de agua subterránea', 1, GETDATE(), 'admin'),
(41, 'Represa', 'Depósito de agua superficial', 1, GETDATE(), 'admin'),

-- Otros
(99, 'Otro tipo de registro', 'Elemento no clasificado', 1, GETDATE(), 'admin');

-- ============================================
-- SEED DATA: Sistema de Unidades de Medida
-- ============================================

-- UnidadesMedida (27 registros con IDs explícitos)
SET IDENTITY_INSERT UnidadesMedida ON;
INSERT INTO UnidadesMedida (Id, Nombre, Sigla, Categoria, DimensionBase, FactorConversion, Orden, Activo)
VALUES
-- PesoPorSuperficie (Categoria 4, DimensionBase 4)
(1,  'Kilogramo por Hectárea',     'Kg/Ha',    4, 4, 1.0,       1, 1),
(7,  'Tonelada por Hectárea',      'Tn/Ha',    4, 4, 1000.0,    2, 1),
(8,  'Gramo por Hectárea',         'g/Ha',     4, 4, 0.001,     3, 1),
(11, 'Libra por Acre',             'Lb/ac',    4, 4, 1.12085,   4, 1),

-- Peso (Categoria 2, DimensionBase 1)
(2,  'Kilogramo',                   'Kg',       2, 1, 1.0,       1, 1),
(12, 'Gramo',                       'g',        2, 1, 0.001,     2, 1),
(25, 'Tonelada',                    'Tn',       2, 1, 1000.0,    3, 1),

-- VolumenPorSuperficie (Categoria 5, DimensionBase 5)
(3,  'Litro por Hectárea',         'Lt/Ha',    5, 5, 1.0,       1, 1),
(9,  'Mililitro por Hectárea',     'mL/Ha',    5, 5, 0.001,     2, 1),
(10, 'Centímetro Cúbico por Hectárea', 'cc/Ha', 5, 5, 0.001,    3, 1),
(23, 'Metro Cúbico por Hectárea',  'm³/Ha',    5, 5, 1000.0,    4, 1),

-- Volumen (Categoria 3, DimensionBase 3)
(4,  'Litro',                       'Lt',       3, 3, 1.0,       1, 1),
(5,  'Metro Cúbico',               'm³',       3, 3, 1000.0,    2, 1),

-- Superficie (Categoria 1, DimensionBase 2)
(6,  'Hectárea',                    'Ha',       1, 2, 1.0,       1, 1),
(24, 'Acre',                        'ac',       1, 2, 0.404686,  2, 1),

-- Concentracion (Categoria 6, DimensionBase 6 y 7)
(15, 'Kilogramo por 100 Litros',    'Kg/100L',  6, 6, 1.0,       1, 1),
(16, 'Litro por 100 Litros',       'L/100L',    6, 7, 1.0,       2, 1),
(17, 'Centímetro Cúbico por 100 Litros', 'cc/100L', 6, 7, 1.0,   3, 1),
(18, 'Mililitro por 100 Litros',   'mL/100L',   6, 7, 1.0,       4, 1),
(19, 'Gramo por 100 Litros',       'g/100L',    6, 6, 1.0,       5, 1),

-- Conteo (Categoria 8, DimensionBase 8 y 9)
(13, 'Unidad por Hectárea',        'Ud/Ha',    8, 9, 1.0,       1, 1),
(14, 'Dosis por Hectárea',         'Dosis/Ha', 8, 9, 1.0,       2, 1),
(20, 'Unidad',                      'Ud',       8, 8, 1.0,       3, 1),
(27, 'Semilla por Hectárea',       'Sem/Ha',   8, 9, 1.0,       4, 1),

-- Lineal (Categoria 7, DimensionBase 10)
(26, 'Semilla por Metro',          'Sem/m',    7, 10, 1.0,      1, 1),

-- Longitud (Categoria 9, DimensionBase 11)
(21, 'Milímetro',                   'mm',       9, 11, 1.0,      1, 1),
(22, 'Pulgada',                     'in',       9, 11, 25.4,     2, 1);
SET IDENTITY_INSERT UnidadesMedida OFF;
GO

-- CamposLaborUnidad (9 registros con IDs explícitos)
SET IDENTITY_INSERT CamposLaborUnidad ON;
INSERT INTO CamposLaborUnidad (Id, IdTipoActividad, NombreCampo, NombrePropiedad, Etiqueta, Requerido, Orden, Activo)
VALUES
(1, 2, 'Densidad',   'Densidad',          'Densidad de siembra', 0, 1, 1),  -- Siembra
(2, 2, 'Superficie', 'Superficie',         'Superficie sembrada', 0, 2, 1), -- Siembra
(3, 3, 'Volumen',    'Volumen',            'Volumen de aplicación', 0, 1, 1), -- Pulverización
(4, 3, 'Dosis',      'Dosis',             'Dosis del producto', 0, 2, 1),    -- Pulverización
(5, 4, 'Cantidad',   'Cantidad',           'Cantidad aplicada', 0, 1, 1),    -- Fertilización
(6, 4, 'Dosis',      'Dosis',             'Dosis del fertilizante', 0, 2, 1), -- Fertilización
(7, 5, 'Volumen',    'VolumenAgua',        'Volumen de agua', 0, 1, 1),      -- Riego
(8, 7, 'Rendimiento','Rendimiento',        'Rendimiento del cultivo', 0, 1, 1), -- Cosecha
(9, 7, 'Superficie', 'SuperficieCosechada','Superficie cosechada', 0, 2, 1); -- Cosecha
SET IDENTITY_INSERT CamposLaborUnidad OFF;
GO

-- CamposLaborUnidadPermitida (37 registros)
INSERT INTO CamposLaborUnidadPermitida (IdCampoLaborUnidad, IdUnidadMedida, EsPredeterminado, Orden)
VALUES
-- Siembra → Densidad (Campo 1) — Default: Kg/Ha
(1, 1,  1, 1),  -- Kg/Ha
(1, 7,  0, 2),  -- Tn/Ha
(1, 8,  0, 3),  -- g/Ha
(1, 11, 0, 4),  -- Lb/ac
(1, 26, 0, 5),  -- Sem/m
(1, 27, 0, 6),  -- Sem/Ha
(1, 13, 0, 7),  -- Ud/Ha

-- Siembra → Superficie (Campo 2) — Default: Ha
(2, 6,  1, 1),  -- Ha
(2, 24, 0, 2),  -- ac

-- Pulverización → Volumen (Campo 3) — Default: Lt/Ha
(3, 3,  1, 1),  -- Lt/Ha
(3, 9,  0, 2),  -- mL/Ha
(3, 10, 0, 3),  -- cc/Ha
(3, 23, 0, 4),  -- m³/Ha

-- Pulverización → Dosis (Campo 4) — Default: Lt/Ha
(4, 3,  1, 1),  -- Lt/Ha
(4, 1,  0, 2),  -- Kg/Ha
(4, 9,  0, 3),  -- mL/Ha
(4, 10, 0, 4),  -- cc/Ha
(4, 8,  0, 5),  -- g/Ha
(4, 7,  0, 6),  -- Tn/Ha
(4, 23, 0, 7),  -- m³/Ha

-- Fertilización → Cantidad (Campo 5) — Default: Kg
(5, 2,  1, 1),  -- Kg
(5, 12, 0, 2),  -- g
(5, 25, 0, 3),  -- Tn

-- Fertilización → Dosis (Campo 6) — Default: Kg/Ha
(6, 1,  1, 1),  -- Kg/Ha
(6, 7,  0, 2),  -- Tn/Ha
(6, 8,  0, 3),  -- g/Ha
(6, 11, 0, 4),  -- Lb/ac
(6, 3,  0, 5),  -- Lt/Ha

-- Riego → Volumen (Campo 7) — Default: m³
(7, 5,  1, 1),  -- m³
(7, 21, 0, 2),  -- mm
(7, 4,  0, 3),  -- Lt
(7, 23, 0, 4),  -- m³/Ha

-- Cosecha → Rendimiento (Campo 8) — Default: Tn/Ha
(8, 7,  1, 1),  -- Tn/Ha
(8, 1,  0, 2),  -- Kg/Ha
(8, 11, 0, 3),  -- Lb/ac

-- Cosecha → Superficie (Campo 9) — Default: Ha
(9, 6,  1, 1),  -- Ha
(9, 24, 0, 2);  -- ac
GO
