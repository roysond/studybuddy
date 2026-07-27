interface StudyMaterialInputProps {
  value: string;
  onChange: (value: string) => void;
  label: string;
}

/**
 * Presentation-only textarea for pasting study material.
 * No business logic and no knowledge of which mode uses it.
 */
export function StudyMaterialInput({
  value,
  onChange,
  label,
}: StudyMaterialInputProps) {
  return (
    <label className="field">
      <span className="field__label">{label}</span>
      <textarea
        className="field__textarea"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        rows={10}
        placeholder="Paste your study material here…"
      />
    </label>
  );
}
