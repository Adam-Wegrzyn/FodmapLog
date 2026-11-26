import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { translateSymptomType } from '../services/reference-i18n';
import {
  InsightFinding,
  InsightRareFood,
  InsightsResponse,
  InsightsService
} from '../services/insights.service';

type VoteState = 'pending' | 'up' | 'down' | 'error';

@Component({
  selector: 'app-insights',
  templateUrl: './insights.component.html',
  styleUrl: './insights.component.css'
})
export class InsightsComponent implements OnInit {
  loading = true;
  loadError = false;
  insight: InsightsResponse | null = null;
  votes: { [foodKey: string]: VoteState } = {};

  constructor(
    private insights: InsightsService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.loadError = false;
    this.insights.getInsights(14).subscribe({
      next: (result) => {
        this.insight = {
          status: result.status,
          days: result.days,
          progress: result.progress,
          findings: result.findings || [],
          rarelyFollowed: result.rarelyFollowed || []
        };
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.loadError = true;
      }
    });
  }

  meter(current: number, required: number): number {
    if (required <= 0) {
      return 0;
    }
    return Math.max(0, Math.min(100, (current / required) * 100));
  }

  labelKey(label: string): string {
    return label === 'stronger pattern' ? 'insights.labelStronger' : 'insights.labelWeak';
  }

  sentence(finding: InsightFinding): string {
    const symptoms = this.symptomList(finding.symptomNames);
    const params = {
      hits: finding.hits,
      exposures: finding.exposures,
      product: finding.product,
      symptoms,
      baselineHits: finding.baselineHits,
      baselineMeals: finding.baselineMeals
    };
    const key = symptoms ? 'insights.finding' : 'insights.findingNoName';
    return this.translate.instant(key, params);
  }

  eatenWith(finding: InsightFinding): string {
    const others = finding.eatenWith || [];
    if (others.length === 0) {
      return '';
    }
    if (others.length === 1) {
      return this.translate.instant('insights.eatenWithOne', {
        product: finding.product,
        other: others[0]
      });
    }
    const and = this.translate.instant('insights.and');
    const last = others[others.length - 1];
    const head = others.slice(0, -1).join(', ');
    return this.translate.instant('insights.eatenWithMany', {
      product: finding.product,
      others: `${head} ${and} ${last}`
    });
  }

  rareSentence(item: InsightRareFood): string {
    return this.translate.instant('insights.rarelyItem', {
      product: item.product,
      exposures: item.exposures
    });
  }

  vote(finding: InsightFinding, helpful: boolean): void {
    const current = this.votes[finding.foodKey];
    if (current === 'pending' || current === 'up' || current === 'down') {
      return;
    }
    this.votes[finding.foodKey] = 'pending';
    this.insights.sendFeedback({
      foodKey: finding.foodKey,
      product: finding.product,
      helpful
    }).subscribe({
      next: () => {
        this.votes[finding.foodKey] = helpful ? 'up' : 'down';
      },
      error: () => {
        this.votes[finding.foodKey] = 'error';
      }
    });
  }

  trackFinding(_: number, finding: InsightFinding): string {
    return finding.foodKey;
  }

  trackRare(_: number, item: InsightRareFood): string {
    return item.foodKey;
  }

  /** Symptom labels are title case in the catalog; the card uses them mid-sentence. */
  private midSentence(name: string): string {
    const trimmed = name.trim();
    if (!trimmed) {
      return '';
    }
    return trimmed.charAt(0).toLocaleLowerCase() + trimmed.slice(1);
  }

  private symptomList(names: string[] | null | undefined): string {
    const translated = (names || [])
      .map(name => translateSymptomType(this.translate, { name }))
      .map(name => this.midSentence(name))
      .filter(name => name.length > 0);
    if (translated.length === 0) {
      return '';
    }
    if (translated.length === 1) {
      return translated[0];
    }
    const or = this.translate.instant('insights.or');
    return translated.join(` ${or} `);
  }
}
