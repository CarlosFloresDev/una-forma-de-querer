using System;
using System.IO;
using UFQ.UseCases.Persistence;
using UnityEngine;

namespace UFQ.Infrastructure.Persistence
{
    /// <summary>
    /// Guarda la partida como JSON. Escribe a un archivo temporal y luego lo reemplaza,
    /// para que un cierre a mitad no corrompa la partida. Las partidas ilegibles o
    /// inválidas se apartan con un nombre único y nunca se borran.
    /// </summary>
    public sealed class JsonNarrativeRepository : INarrativeRepository
    {
        private readonly string _path;

        /// <summary>Ruta explícita (útil en pruebas o para varias ranuras de guardado).</summary>
        public JsonNarrativeRepository(string fullPath)
        {
            _path = fullPath;
        }

        /// <summary>Partida en la carpeta de datos del jugador (persistentDataPath).</summary>
        public static JsonNarrativeRepository InPersistentData(string fileName = "partida.json") =>
            new JsonNarrativeRepository(Path.Combine(UnityEngine.Application.persistentDataPath, fileName));

        public bool TryLoad(out NarrativeSnapshot snapshot)
        {
            snapshot = null;
            if (!File.Exists(_path)) return false;

            try
            {
                snapshot = JsonUtility.FromJson<NarrativeSnapshot>(File.ReadAllText(_path));
            }
            catch (Exception e)
            {
                Quarantine($"JSON ilegible: {e.Message}");
                return false;
            }

            if (snapshot != null) return true;
            Quarantine("JSON vacío");
            return false;
        }

        public void Save(NarrativeSnapshot snapshot)
        {
            var tmp = _path + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
            File.WriteAllText(tmp, JsonUtility.ToJson(snapshot, true));

            if (File.Exists(_path))
            {
                File.Replace(tmp, _path, null);
            }
            else
            {
                File.Move(tmp, _path);
            }
        }

        public void Delete()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }

        public void Quarantine(string reason)
        {
            if (!File.Exists(_path)) return;

            var target = $"{_path}.apartada-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            try
            {
                // Mover (no copiar): así el siguiente arranque no repite el mismo fallo.
                File.Move(_path, target);
                Debug.LogWarning($"[UFQ] Partida apartada en {target}. Motivo: {reason}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UFQ] No se pudo apartar la partida ({reason}): {e.Message}");
            }
        }
    }
}
