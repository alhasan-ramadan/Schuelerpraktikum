namespace StudentSandbox
{
    /// <summary>
    /// Alles, was man im Play Mode mit der linken Maustaste anklicken kann.
    /// Ein eigenes Skript braucht nur diese eine Methode, damit der <see cref="ClickInteractor"/> es findet.
    /// Das Objekt (oder eines seiner Eltern-Objekte) braucht außerdem einen Collider.
    /// </summary>
    public interface IClickable
    {
        void OnClick();
    }

    /// <summary>Optional: Objekte, die reagieren, wenn die Maus darüber fährt.</summary>
    public interface IHoverable
    {
        /// <summary>Kurzer Text, der neben dem Mauszeiger erscheint.</summary>
        string HoverText { get; }
        void SetHovered(bool hovered);
    }

    /// <summary>
    /// Alles, was der Reset-Knopf (Taste R) wieder in den Startzustand bringen soll.
    /// Der <see cref="PhysicsResetter"/> findet automatisch alle Komponenten mit diesem Interface.
    /// </summary>
    public interface IResettable
    {
        void ResetToStart();
    }
}
