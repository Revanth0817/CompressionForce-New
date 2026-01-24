namespace CompressionForce.Domain.Entities
{
    /// <summary>
    /// Domain representation of recipe audit history.
    /// Stored as JSON snapshots, not field-level diffs.
    /// </summary>
    public class RecipeHistory
    {
        public string RecipeCode { get; }
        public string Action { get; } // ADD, UPDATE, DELETE
        public string ChangedBy { get; }
        public DateTime ChangedAt { get; }
        public string OldParametersJson { get; }
        public string NewParametersJson { get; }

        public RecipeHistory(
            string recipeCode,
            string action,
            string changedBy,
            string oldParametersJson,
            string newParametersJson)
        {
            RecipeCode = recipeCode;
            Action = action;
            ChangedBy = changedBy;
            ChangedAt = DateTime.UtcNow;
            OldParametersJson = oldParametersJson;
            NewParametersJson = newParametersJson;
        }
    }
}
