import { TranslateService } from '@ngx-translate/core';

/** Canonical English seed names → DB ids (DataAccess seeds). */
const SYMPTOM_NAME_TO_ID: Record<string, number> = {
  nausea: 1,
  burping: 2,
  diarrhea: 3,
  constipation: 4,
  bloating: 5,
  'abdominal pain': 6,
  heartburn: 7,
  gas: 8,
  cramps: 9,
  vomiting: 10,
  appetite: 11,
  headache: 12,
  fatigue: 13,
  mood: 14,
  energy: 15,
  'sleep quality': 16,
  stress: 17,
  concentration: 18,
  motivation: 19,
  'physical activity': 20,
  'general well-being': 21,
  'general wellbeing': 21
};

const UNIT_NAME_TO_ID: Record<string, number> = {
  gram: 1,
  kilogram: 2,
  milligram: 3,
  liter: 4,
  litre: 4,
  milliliter: 5,
  millilitre: 5,
  teaspoon: 6,
  tablespoon: 7,
  cup: 8,
  piece: 9,
  slice: 10,
  drop: 11,
  pinch: 12,
  ounce: 13,
  pound: 14,
  'fluid ounce': 15,
  pint: 16,
  quart: 17,
  gallon: 18,
  can: 19,
  package: 20,
  bottle: 21
};

export function resolveSymptomTypeId(type?: { id?: number; name?: string } | null): number | null {
  if (type?.id && type.id > 0) {
    return type.id;
  }
  const name = (type?.name || '').trim().toLowerCase();
  return name ? SYMPTOM_NAME_TO_ID[name] ?? null : null;
}

export function resolveUnitId(unit?: { id?: number; name?: string } | null): number | null {
  if (unit?.id && unit.id > 0) {
    return unit.id;
  }
  const name = (unit?.name || '').trim().toLowerCase();
  return name ? UNIT_NAME_TO_ID[name] ?? null : null;
}

export function translateSymptomType(
  translate: TranslateService,
  type?: { id?: number; name?: string } | null
): string {
  const id = resolveSymptomTypeId(type);
  if (id != null) {
    const key = `ref.symptom.${id}`;
    const value = translate.instant(key);
    if (value !== key) {
      return value;
    }
  }
  return type?.name || '';
}

export function translateUnit(
  translate: TranslateService,
  unit?: { id?: number; name?: string } | null
): string {
  const id = resolveUnitId(unit);
  if (id != null) {
    const key = `ref.unit.${id}`;
    const value = translate.instant(key);
    if (value !== key) {
      return value;
    }
  }
  return unit?.name || '';
}

export function translateScale(translate: TranslateService, scale: number): string {
  const key = `ref.scale.${scale}`;
  const value = translate.instant(key);
  return value !== key ? value : `${scale}`;
}
