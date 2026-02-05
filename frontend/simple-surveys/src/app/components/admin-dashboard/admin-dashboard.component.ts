import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { SurveyListItem } from '../../models/survey.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  surveys = signal<SurveyListItem[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  deleteConfirm = signal<string | null>(null);
  copiedId = signal<string | null>(null);

  constructor(
    private api: ApiService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadSurveys();
  }

  loadSurveys(): void {
    this.loading.set(true);
    this.api.getAdminSurveys().subscribe({
      next: (surveys) => {
        this.surveys.set(surveys);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load surveys');
        this.loading.set(false);
      }
    });
  }

  confirmDelete(id: string): void {
    this.deleteConfirm.set(id);
  }

  cancelDelete(): void {
    this.deleteConfirm.set(null);
  }

  deleteSurvey(id: string): void {
    this.api.deleteSurvey(id).subscribe({
      next: () => {
        this.deleteConfirm.set(null);
        this.loadSurveys();
      },
      error: () => {
        this.error.set('Failed to delete survey');
      }
    });
  }

  copyLink(id: string): void {
    const url = `${window.location.origin}/survey/${id}`;
    navigator.clipboard.writeText(url).then(() => {
      this.copiedId.set(id);
      setTimeout(() => this.copiedId.set(null), 2000);
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
