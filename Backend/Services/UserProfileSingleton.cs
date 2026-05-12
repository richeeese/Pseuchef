// ============================================================
// UserProfileSingleton.cs  —  Backend/Models/
//
// Global state holder for the logged-in user's diet and
// intolerances. Wraps the existing UserProfile class so
// nothing in the backend needs to change.
//
// USAGE:
//   // Read anywhere in the app:
//   var diet        = UserProfileSingleton.Instance.Diet;
//   var intolerances = UserProfileSingleton.Instance.Intolerances;
//
//   // Write from the ProfileForm:
//   UserProfileSingleton.Instance.SetDiet("vegetarian");
//   UserProfileSingleton.Instance.ToggleIntolerance("dairy");
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    public sealed class UserProfileSingleton
    {
        // ── Singleton plumbing ───────────────────────────────────────
        private static readonly Lazy<UserProfileSingleton> _instance =
            new(() => new UserProfileSingleton());

        public static UserProfileSingleton Instance => _instance.Value;

        private UserProfileSingleton() { Load(); }

        // ── State ────────────────────────────────────────────────────

        /// <summary>Username shown in the header / greeting.</summary>
        public string Username { get; private set; } = "Chef";

        /// <summary>
        /// Spoonacular diet parameter value, e.g. "vegetarian", "vegan".
        /// Empty string means no diet filter (None).
        /// </summary>
        public string Diet { get; private set; } = "";

        /// <summary>
        /// Spoonacular intolerances list, e.g. ["dairy", "gluten"].
        /// </summary>
        public List<string> Intolerances { get; private set; } = new();

        // ── Diet options shown in the UI ComboBox ────────────────────
        // Key = display label, Value = Spoonacular API string
        public static readonly Dictionary<string, string> DietOptions = new()
        {
            ["None"]       = "",
            ["Vegetarian"] = "vegetarian",
            ["Vegan"]      = "vegan",
        };

        // ── Intolerance options shown as checkboxes ──────────────────
        // These match Spoonacular's accepted intolerance strings exactly.
        public static readonly List<string> IntoleranceOptions = new()
        {
            "dairy", "gluten", "peanut", "seafood", "soy"
        };

        // ── Mutators ─────────────────────────────────────────────────

        public void SetUsername(string name)
        {
            Username = string.IsNullOrWhiteSpace(name) ? "Chef" : name.Trim();
        }

        /// <param name="displayLabel">Key from DietOptions, e.g. "Vegetarian"</param>
        public void SetDiet(string displayLabel)
        {
            Diet = DietOptions.TryGetValue(displayLabel, out var api) ? api : "";
        }

        public void ToggleIntolerance(string intolerance)
        {
            if (Intolerances.Contains(intolerance))
                Intolerances.Remove(intolerance);
            else
                Intolerances.Add(intolerance);
        }

        public void SetIntolerance(string intolerance, bool enabled)
        {
            if (enabled && !Intolerances.Contains(intolerance))
                Intolerances.Add(intolerance);
            else if (!enabled)
                Intolerances.Remove(intolerance);
        }

        // ── Helpers for the API layer ─────────────────────────────────

        /// <summary>Returns the diet API string, or "" if none.</summary>
        public string GetDietParam() => Diet;

        /// <summary>Returns comma-joined intolerances for Spoonacular, or "".</summary>
        public string GetIntolerancesParam() =>
            Intolerances.Count > 0 ? string.Join(",", Intolerances) : "";

        /// <summary>True when at least one filter is active.</summary>
        public bool HasFilters => !string.IsNullOrEmpty(Diet) || Intolerances.Count > 0;

        // ── Persistence (simple JSON to AppData) ─────────────────────
        private static string SavePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Pseuchef", "profile.json");

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
                var data = new ProfileData
                {
                    Username    = Username,
                    Diet        = Diet,
                    Intolerances = Intolerances
                };
                File.WriteAllText(SavePath,
                    JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* non-critical */ }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                var data = JsonSerializer.Deserialize<ProfileData>(File.ReadAllText(SavePath));
                if (data == null) return;
                Username     = data.Username    ?? "Chef";
                Diet         = data.Diet        ?? "";
                Intolerances = data.Intolerances ?? new();
            }
            catch { /* corrupt file — use defaults */ }
        }

        private class ProfileData
        {
            public string Username     { get; set; } = "Chef";
            public string Diet         { get; set; } = "";
            public List<string> Intolerances { get; set; } = new();
        }
    }
}
