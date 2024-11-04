namespace OoLunar.Tomoe.Interactivity.Data
{
    public record ConfirmData : IdleData
    {
        public required string Question { get; init; }
        public bool Response { get; set; }
    }
}
