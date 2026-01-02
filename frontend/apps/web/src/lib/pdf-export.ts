/**
 * Utility for exporting data to PDF using browser print functionality
 */

export function exportToPDF(elementId: string, filename: string) {
  const printWindow = window.open('', '_blank');
  if (!printWindow) {
    throw new Error('Unable to open print window. Please allow popups.');
  }

  const element = document.getElementById(elementId);
  if (!element) {
    throw new Error(`Element with id "${elementId}" not found`);
  }

  const html = `
    <!DOCTYPE html>
    <html>
      <head>
        <title>${filename}</title>
        <style>
          body {
            font-family: system-ui, -apple-system, sans-serif;
            padding: 20px;
            max-width: 1200px;
            margin: 0 auto;
          }
          @media print {
            body { padding: 0; }
            .no-print { display: none !important; }
          }
          h1 { color: #1f2937; margin-bottom: 10px; }
          h2 { color: #374151; margin-top: 20px; margin-bottom: 10px; }
          h3 { color: #4b5563; margin-top: 15px; margin-bottom: 8px; }
          p { color: #6b7280; margin: 5px 0; }
          table { border-collapse: collapse; width: 100%; margin: 15px 0; }
          th, td { border: 1px solid #e5e7eb; padding: 8px; text-align: left; }
          th { background-color: #f3f4f6; font-weight: 600; }
          .card { border: 1px solid #e5e7eb; border-radius: 8px; padding: 15px; margin: 15px 0; }
          .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
          .badge-green { background-color: #d1fae5; color: #065f46; }
          .badge-yellow { background-color: #fef3c7; color: #92400e; }
          .badge-red { background-color: #fee2e2; color: #991b1b; }
          .metric-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 15px 0; }
          .metric-card { border: 1px solid #e5e7eb; border-radius: 8px; padding: 15px; text-align: center; }
          .metric-value { font-size: 24px; font-weight: bold; color: #1f2937; }
          .metric-label { font-size: 14px; color: #6b7280; margin-top: 5px; }
          .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; color: #9ca3af; font-size: 12px; }
        </style>
      </head>
      <body>
        ${element.innerHTML}
        <div class="footer">
          <p>Relatório gerado em ${new Date().toLocaleDateString('pt-BR', { day: '2-digit', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</p>
          <p>TaktIQ - Sistema de Gestão de Personal Training</p>
        </div>
      </body>
    </html>
  `;

  printWindow.document.write(html);
  printWindow.document.close();

  // Wait for content to load before printing
  printWindow.onload = () => {
    setTimeout(() => {
      printWindow.print();
      // Don't close immediately - user might want to preview
      printWindow.onafterprint = () => {
        printWindow.close();
      };
    }, 250);
  };
}

export function preparePrintableStats(stats: any, studentName: string, period: string): string {
  return `
    <div id="printable-stats">
      <h1>Relatório de Estatísticas - ${studentName}</h1>
      <p><strong>Período:</strong> ${period}</p>

      <div class="metric-grid">
        <div class="metric-card">
          <div class="metric-value">${stats.completionRate.toFixed(1)}%</div>
          <div class="metric-label">Taxa de Conclusão</div>
          <p style="font-size: 12px; margin-top: 8px;">${stats.completedWorkouts} de ${stats.totalWorkoutsScheduled} treinos</p>
        </div>

        ${stats.averageDifficulty !== null ? `
        <div class="metric-card">
          <div class="metric-value">${stats.averageDifficulty.toFixed(1)}/5</div>
          <div class="metric-label">Dificuldade Média</div>
        </div>
        ` : ''}

        ${stats.averageEnergy !== null ? `
        <div class="metric-card">
          <div class="metric-value">${stats.averageEnergy.toFixed(1)}/5</div>
          <div class="metric-label">Energia Média</div>
        </div>
        ` : ''}

        ${stats.averageSatisfaction !== null ? `
        <div class="metric-card">
          <div class="metric-value">${stats.averageSatisfaction.toFixed(1)}/5</div>
          <div class="metric-label">Satisfação Média</div>
        </div>
        ` : ''}
      </div>

      ${stats.frequentPainAreas && stats.frequentPainAreas.length > 0 ? `
      <div class="card">
        <h2>⚠️ Áreas de Dor Frequentes</h2>
        <table>
          <thead>
            <tr>
              <th>Área</th>
              <th>Ocorrências</th>
              <th>Percentual</th>
            </tr>
          </thead>
          <tbody>
            ${stats.frequentPainAreas.map((pain: any) => `
              <tr>
                <td>${pain.area}</td>
                <td>${pain.count}x</td>
                <td>${pain.percentage.toFixed(1)}%</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
        <p style="margin-top: 10px; color: #dc2626;">
          <strong>Recomendação:</strong> Considere ajustar os treinos para reduzir sobrecarga nessas áreas.
        </p>
      </div>
      ` : ''}

      ${stats.recentFeedback && stats.recentFeedback.length > 0 ? `
      <div class="card">
        <h2>💬 Feedback Recente</h2>
        ${stats.recentFeedback.slice(0, 5).map((fb: any) => `
          <div style="border-bottom: 1px solid #e5e7eb; padding: 10px 0;">
            <p><strong>${fb.workoutName}</strong> - ${new Date(fb.date).toLocaleDateString('pt-BR')}</p>
            <p>Dificuldade: ${fb.difficulty}/5 | Satisfação: ${fb.satisfaction}/5</p>
            ${fb.comments ? `<p style="font-style: italic; color: #6b7280;">"${fb.comments}"</p>` : ''}
          </div>
        `).join('')}
      </div>
      ` : ''}
    </div>
  `;
}
