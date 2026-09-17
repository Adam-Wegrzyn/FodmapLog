import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { FodmapLogService } from '../services/fodmap-log-service';
import { DailyLog } from '../domain/DailyLog';
import { SymptomScale } from '../domain/SymptomScale';

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

  private readonly symptomScale = SymptomScale;

  constructor(
    private fodmapLogService: FodmapLogService,
    private router: Router
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
      this.errorMessage = 'Choose both a start and end date.';
      return;
    }
    if (this.toDate < this.fromDate) {
      this.errorMessage = 'End date must be on or after the start date.';
      return;
    }

    const daySpan =
      (new Date(this.toDate + 'T12:00:00').getTime() -
        new Date(this.fromDate + 'T12:00:00').getTime()) /
      (1000 * 60 * 60 * 24);
    if (daySpan > 366) {
      this.errorMessage = 'Please keep the range within 366 days.';
      return;
    }

    this.isExporting = true;
    this.fodmapLogService.getDailyLogsByDateRange(this.fromDate, this.toDate).subscribe({
      next: (logs) => {
        try {
          const rows = this.buildRows(logs || []);
          this.previewCount = rows.length;
          this.writePdf(rows);
        } catch (err) {
          console.error(err);
          this.errorMessage = 'Could not create the PDF. Please try again.';
        } finally {
          this.isExporting = false;
        }
      },
      error: (err) => {
        console.error(err);
        this.isExporting = false;
        this.errorMessage = 'Could not load logs for that range.';
      }
    });
  }

  private buildRows(logs: DailyLog[]): string[][] {
    const rows: string[][] = [];
    const sorted = [...logs].sort(
      (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
    );

    for (const log of sorted) {
      const when = this.formatWhen(log.date);
      if (log.mealLog?.productQuantity?.length) {
        const foods = log.mealLog.productQuantity
          .map(pq => {
            const qty = pq.quantity ?? '';
            const unit = (pq.unit?.name || '').trim();
            const name = pq.product?.name || 'Food';
            return unit ? `${name} (${qty} ${unit})` : `${name} (${qty})`;
          })
          .join(', ');
        rows.push([when, 'Meal', foods]);
      } else if (log.symptomsLog?.symptoms?.length) {
        const symptoms = log.symptomsLog.symptoms
          .map(s => {
            const name = s.symptomType?.name || 'Symptom';
            const scale = this.symptomScale[s.symptomScale] ?? `${s.symptomScale}`;
            return `${name} (${scale})`;
          })
          .join(', ');
        rows.push([when, 'Symptom', symptoms]);
      }
    }
    return rows;
  }

  private writePdf(rows: string[][]): void {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });
    const margin = 36;
    const pageWidth = doc.internal.pageSize.getWidth();

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.text('HealthyGutLog export', margin, 48);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(80);
    doc.text(`From ${this.fromDate} to ${this.toDate}`, margin, 66);
    doc.text(`Generated ${this.toInputDate(new Date())}`, margin, 80);
    doc.setTextColor(0);

    if (rows.length === 0) {
      doc.text('No meals or symptoms in this date range.', margin, 110);
    } else {
      autoTable(doc, {
        startY: 96,
        head: [['Date / time', 'Type', 'Details']],
        body: rows,
        margin: { left: margin, right: margin },
        styles: {
          font: 'helvetica',
          fontSize: 9,
          cellPadding: 6,
          overflow: 'linebreak',
          valign: 'top'
        },
        headStyles: {
          fillColor: [31, 122, 77],
          textColor: 255,
          fontStyle: 'bold'
        },
        columnStyles: {
          0: { cellWidth: 90 },
          1: { cellWidth: 60 },
          2: { cellWidth: pageWidth - margin * 2 - 150 }
        },
        didDrawPage: (data) => {
          const page = doc.getNumberOfPages();
          doc.setFontSize(8);
          doc.setTextColor(120);
          doc.text(
            `Page ${page}`,
            pageWidth - margin,
            doc.internal.pageSize.getHeight() - 16,
            { align: 'right' }
          );
        }
      });
    }

    const fileName = `healthygutlog-${this.fromDate}-to-${this.toDate}.pdf`;
    // save() triggers download on mobile browsers (Files / Downloads / Share sheet).
    doc.save(fileName);
  }

  private formatWhen(value: string): string {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) {
      return value;
    }
    const date = d.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
    const time = d.toLocaleTimeString(undefined, {
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
