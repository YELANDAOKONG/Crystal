namespace Crystal.Generation;

/// <summary>Contains one exact caller-authored text generation input.</summary>
public sealed record GenerationTextInput : GenerationInput
{
    /// <summary>Initializes an exact text instruction.</summary>
    /// <param name="text">The exact caller-authored text.</param>
    /// <param name="purpose">
    /// Its instruction, reference, or source purpose. The default is
    /// instruction.
    /// </param>
    public GenerationTextInput(
        string text,
        GenerationInputPurpose? purpose = null)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        var effectivePurpose = purpose ?? GenerationInputPurpose.Instruction;
        GenerationInputRules.Validate(
            ContentModality.Text,
            effectivePurpose,
            nameof(purpose));

        Text = text;
        Purpose = effectivePurpose;
    }

    /// <inheritdoc />
    public override ContentModality Modality => ContentModality.Text;

    /// <inheritdoc />
    public override GenerationInputPurpose Purpose { get; }

    /// <summary>Gets the exact caller-authored text.</summary>
    public string Text { get; }
}
