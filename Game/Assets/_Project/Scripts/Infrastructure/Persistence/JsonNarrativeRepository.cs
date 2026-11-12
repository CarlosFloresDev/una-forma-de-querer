using System;
using System.IO;
using UFQ.UseCases.Persistence;
using UnityEngine;

namespace UFQ.Infrastructure.Persistence
{
    /// <summary>
    /// Guarda la partida como JSON en persistentDataPath. Escribe primero a un archivo
    /// temporal y luego lo reemplaza, para que un cierre a mitad no corrompa la partida.
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
                return snapshot != null;
            }
            catch (Exception e)
            {
                // Una partida ilegible no debe impedir jugar: se aparta y se empieza de cero.
                Debug.LogError($"[UFQ] Partida dañada, se aparta: {e.Message}");
                File.Copy(_path, _path + ".corrupta", true);
                return false;
            }
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
    }
}
