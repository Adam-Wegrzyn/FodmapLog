export class AudioBase64 {
  value: string;
  /** Azure Speech locale, e.g. pl-PL or en-US. */
  language?: string;

  constructor(value: string, language?: string) {
    this.value = value;
    this.language = language;
  }
}
