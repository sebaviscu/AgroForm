using AgroForm.Model.Unidades;

namespace AgroForm.Business.Services
{
    public static class UnidadConversionHelper
    {
        /// <summary>
        /// Convierte un valor de una unidad dada a su valor canónico (FactorConversion = 1).
        /// Útil para normalizar valores en reportes y cálculos.
        /// </summary>
        public static decimal ACanonico(decimal valor, UnidadMedida unidad)
        {
            if (unidad.FactorConversion == 0m)
                throw new InvalidOperationException($"La unidad '{unidad.Nombre}' tiene FactorConversion = 0.");

            return valor * unidad.FactorConversion;
        }

        /// <summary>
        /// Convierte un valor desde una unidad origen a una unidad destino.
        /// Ambas unidades deben pertenecer a la misma CategoriaUnidad.
        /// Lanza InvalidOperationException si las categorías no coinciden.
        /// </summary>
        public static decimal Convertir(decimal valor, UnidadMedida desde, UnidadMedida hacia)
        {
            if (desde.Categoria != hacia.Categoria)
                throw new InvalidOperationException(
                    $"No se puede convertir entre categorías incompatibles: " +
                    $"'{desde.Nombre}' ({desde.Categoria}) → '{hacia.Nombre}' ({hacia.Categoria}).");

            if (desde.FactorConversion == 0m)
                throw new InvalidOperationException($"La unidad origen '{desde.Nombre}' tiene FactorConversion = 0.");

            if (hacia.FactorConversion == 0m)
                throw new InvalidOperationException($"La unidad destino '{hacia.Nombre}' tiene FactorConversion = 0.");

            // valor_origen * factor_origen = valor_canonico
            // valor_destino = valor_canonico / factor_destino
            decimal valorCanonico = valor * desde.FactorConversion;
            return valorCanonico / hacia.FactorConversion;
        }
    }
}
