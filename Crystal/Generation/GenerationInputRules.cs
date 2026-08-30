namespace Crystal.Generation;

internal static class GenerationInputRules
{
    public static void Validate(
        ContentModality modality,
        GenerationInputPurpose purpose,
        string parameterName)
    {
        var isValid = modality == ContentModality.Text
            ? purpose == GenerationInputPurpose.Instruction
                || purpose == GenerationInputPurpose.Reference
                || purpose == GenerationInputPurpose.Source
            : modality == ContentModality.Image
                ? purpose == GenerationInputPurpose.Reference
                    || purpose == GenerationInputPurpose.Source
                    || purpose == GenerationInputPurpose.Mask
                    || purpose == GenerationInputPurpose.FirstFrame
                    || purpose == GenerationInputPurpose.LastFrame
                : purpose == GenerationInputPurpose.Reference
                    || purpose == GenerationInputPurpose.Source;

        if (!isValid)
        {
            throw new ArgumentException(
                "The generation input purpose is not valid for the modality.",
                parameterName);
        }
    }
}
