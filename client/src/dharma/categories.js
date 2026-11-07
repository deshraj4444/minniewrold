/** Hindi labels for the English category keys stored with each post. */
export const categoryLabels = {
  Mantras: 'मंत्र · Mantras',
  Stotras: 'स्तोत्र व आरती · Stotras',
  Temples: 'मंदिर · Temples'
};

export const categoryLabel = (c) => categoryLabels[c] ?? c;

export const isHindi = (text) => /[ऀ-ॿ]/.test(text ?? '');
