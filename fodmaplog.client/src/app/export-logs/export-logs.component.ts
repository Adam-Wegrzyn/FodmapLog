import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { TranslateService } from '@ngx-translate/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { DailyLog } from '../domain/DailyLog';
import { LanguageService } from '../services/language.service';
import { translateScale, translateSymptomType, translateUnit } from '../services/reference-i18n';

@Component({
  selector: 'app-export-logs',
  templateUrl: './export-logs.component.html',
  styleUrl: './export-logs.component.css'
})
export class ExportLogsComponent implements OnInit {
  fromDate = '';
  toDate = '';
  isExporting = false;
  errorMessage: string | null = null;
  previewCount: number | null = null;

  constructor(
    private fodmapLogService: FodmapLogService,
    private router: Router,
    private translate: TranslateService,
    private language: LanguageService
  ) {}

  ngOnInit(): void {
    const today = this.toInputDate(new Date());
    const monthAgo = new Date();
    monthAgo.setDate(monthAgo.getDate() - 30);
    this.toDate = today;
    this.fromDate = this.toInputDate(monthAgo);
  }

  cancel(): void {
    this.router.navigate(['/daily-log']);
  }

  exportPdf(): void {
    this.errorMessage = null;
    this.previewCount = null;

    if (!this.fromDate || !this.toDate) {
      this.errorMessage = this.translate.instant('export.needDates');
      return;
    }
    if (this.toDate < this.fromDate) {
      this.errorMessage = this.translate.instant('export.orderInvalid');
      return;
    }

    const daySpan =
      (new Date(this.toDate + 'T12:00:00').getTime() -
        new Date(this.fromDate + 'T12:00:00').getTime()) /
      (1000 * 60 * 60 * 24);
    if (daySpan > 366) {
      this.errorMessage = this.translate.instant('export.tooLong');
      return;
    }

    this.isExporting = true;
    this.fodmapLogService.getDailyLogsByDateRange(this.fromDate, this.toDate).subscribe({
      next: async (logs) => {
        try {
          const rows = this.buildRows(logs || []);
          this.previewCount = rows.length;
          await this.writePdf(rows);
        } catch (err) {
          console.error(err);
          this.errorMessage = this.translate.instant('export.pdfFail');
        } finally {
          this.isExporting = false;
        }
      },
      error: (err) => {
        console.error(err);
        this.isExporting = false;
        this.errorMessage = this.translate.instant('export.loadFail');
      }
    });
  }

  private buildRows(logs: DailyLog[]): string[][] {
    const rows: string[][] = [];
    const sorted = [...logs].sort(
      (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
    );
    const mealType = this.translate.instant('export.typeMeal');
    const symptomType = this.translate.instant('export.typeSymptom');

    for (const log of sorted) {
      const when = this.formatWhen(log.date);
      if (log.mealLog?.productQuantity?.length) {
        const foods = log.mealLog.productQuantity
          .map(pq => {
            const qty = pq.quantity ?? '';
            const unit = this.unitLabel(pq.unit);
            const name = pq.product?.name || 'Food';
            return unit ? `${name} (${qty} ${unit})` : `${name} (${qty})`;
          })
          .join(', ');
        rows.push([when, mealType, foods]);
      } else if (log.symptomsLog?.symptoms?.length) {
        const symptoms = log.symptomsLog.symptoms
          .map(s => {
            const name = this.symptomLabel(s.symptomType);
            const scale = this.scaleLabel(s.symptomScale);
            return `${name} (${scale})`;
          })
          .join(', ');
        rows.push([when, symptomType, symptoms]);
      }
    }
    return rows;
  }

  private async writePdf(rows: string[][]): Promise<void> {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });
    const margin = 36;
    const pageWidth = doc.internal.pageSize.getWidth();
    const fontName = await this.ensureUnicodeFont(doc);

    doc.setFont(fontName, 'normal');
    doc.setFontSize(16);
    doc.text(this.translate.instant('export.pdfTitle'), margin, 48);

    doc.setFontSize(10);
    doc.setTextColor(80);
    doc.text(
      this.translate.instant('export.pdfFromTo', { from: this.fromDate, to: this.toDate }),
      margin,
      66
    );
    doc.text(
      this.translate.instant('export.pdfGenerated', { date: this.toInputDate(new Date()) }),
      margin,
      80
    );
    doc.setTextColor(0);

    if (rows.length === 0) {
      doc.text(this.translate.instant('export.pdfEmpty'), margin, 110);
    } else {
      autoTable(doc, {
        startY: 96,
        head: [[
          this.translate.instant('export.colWhen'),
          this.translate.instant('export.colType'),
          this.translate.instant('export.colDetails')
        ]],
        body: rows,
        margin: { left: margin, right: margin },
        styles: {
          font: fontName,
          fontSize: 9,
          cellPadding: 6,
          overflow: 'linebreak',
          valign: 'top'
        },
        headStyles: {
          fillColor: [31, 122, 77],
          textColor: 255,
          fontStyle: 'normal',
          font: fontName
        },
        columnStyles: {
          0: { cellWidth: 90 },
          1: { cellWidth: 60 },
          2: { cellWidth: pageWidth - margin * 2 - 150 }
        },
        didDrawPage: () => {
          const page = doc.getNumberOfPages();
          doc.setFont(fontName, 'normal');
          doc.setFontSize(8);
          doc.setTextColor(120);
          doc.text(
            this.translate.instant('export.page', { page }),
            pageWidth - margin,
            doc.internal.pageSize.getHeight() - 16,
            { align: 'right' }
          );
        }
      });
    }

    const fileName = `healthygutlog-${this.fromDate}-to-${this.toDate}.pdf`;
    doc.save(fileName);
  }

  /** Noto Sans so Polish diacritics render in the PDF. */
  private async ensureUnicodeFont(doc: jsPDF): Promise<string> {
    const res = await fetch('assets/fonts/NotoSans-Regular.ttf');
    if (!res.ok) {
      return 'helvetica';
    }
    const bytes = new Uint8Array(await res.arrayBuffer());
    let binary = '';
    const chunk = 0x8000;
    for (let i = 0; i < bytes.length; i += chunk) {
      binary += String.fromCharCode(...bytes.subarray(i, i + chunk));
    }
    const base64 = btoa(binary);
    doc.addFileToVFS('NotoSans-Regular.ttf', base64);
    doc.addFont('NotoSans-Regular.ttf', 'NotoSans', 'normal');
    return 'NotoSans';
  }

  private unitLabel(unit?: { id?: number; name?: string } | null): string {
    return translateUnit(this.translate, unit);
  }

  private symptomLabel(type?: { id?: number; name?: string } | null): string {
    return translateSymptomType(this.translate, type) || this.translate.instant('daily.symptom');
  }

  private scaleLabel(scale: number): string {
    return translateScale(this.translate, scale);
  }

  private formatWhen(value: string): string {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) {
      return value;
    }
    const locale = this.language.dateLocale;
    const date = d.toLocaleDateString(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
    const time = d.toLocaleTimeString(locale, {
      hour: '2-digit',
      minute: '2-digit'
    });
    return `${date} ${time}`;
  }

  private toInputDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
