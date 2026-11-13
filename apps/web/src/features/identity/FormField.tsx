import { useState } from 'react'
export function FormField({ id, label, value, onChange, error, type = 'text', autoComplete, maxLength = 254, hint, showLabel, hideLabel, required = true }:
  { id: string; label: string; value: string; onChange: (value: string) => void; error?: string;
    type?: string; autoComplete?: string; maxLength?: number; hint?: string; showLabel?: string; hideLabel?: string; required?: boolean }) {
  const [visible, setVisible] = useState(false)
  return <div className="form-field">
    <label htmlFor={id}>{label}</label>
    <div className="input-wrap"><input id={id} name={id} type={type === 'password' && visible ? 'text' : type}
      value={value} onChange={event => onChange(event.target.value)} autoComplete={autoComplete}
      maxLength={maxLength} required={required} aria-invalid={!!error}
      aria-describedby={error ? id + '-error' : hint ? id + '-hint' : undefined} />
      {type === 'password' && <button type="button" className="password-toggle"
        aria-label={visible ? hideLabel : showLabel} onClick={() => setVisible(!visible)}>{visible ? '◉' : '◎'}</button>}
    </div>
    {hint && !error && <small id={id + '-hint'}>{hint}</small>}
    {error && <small className="field-error" id={id + '-error'}>{error}</small>}
  </div>
}
