import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { Survey } from '../../models/survey.models';

@Component({
  selector: 'app-survey-view',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './survey-view.component.html',
  styleUrl: './survey-view.component.scss'
})
export class SurveyViewComponent implements OnInit {
  survey = signal<Survey | null>(null);
  selectedOptions = signal<Set<string>>(new Set());
  voterName = signal('');
  loading = signal(true);
  notFound = signal(false);
  submitting = signal(false);

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSurvey(id);
    }
  }

  loadSurvey(id: string): void {
    this.loading.set(true);
    this.notFound.set(false);

    this.api.getSurvey(id).subscribe({
      next: (survey) => {
        this.survey.set(survey);
        if (survey.currentVoterName) {
          this.voterName.set(survey.currentVoterName);
        }
        this.loading.set(false);
        this.loadMyVotes(id);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      }
    });
  }

  loadMyVotes(surveyId: string): void {
    this.api.getMyVotes(surveyId).subscribe({
      next: (response) => {
        this.selectedOptions.set(new Set(response.optionIds));
      }
    });
  }

  toggleOption(optionId: string): void {
    const survey = this.survey();
    if (!survey || !survey.isActive) return;

    const selected = new Set(this.selectedOptions());

    if (survey.selectionMode === 'Single') {
      selected.clear();
      selected.add(optionId);
    } else {
      if (selected.has(optionId)) {
        selected.delete(optionId);
      } else {
        selected.add(optionId);
      }
    }

    this.selectedOptions.set(selected);
  }

  isSelected(optionId: string): boolean {
    return this.selectedOptions().has(optionId);
  }

  submitVote(): void {
    const survey = this.survey();
    if (!survey || this.selectedOptions().size === 0) return;

    const name = this.voterName().trim();
    if (!name) {
      this.toast.error('Please enter your name');
      return;
    }

    this.submitting.set(true);

    this.api.vote(survey.id, {
      optionIds: Array.from(this.selectedOptions()),
      voterName: name
    }).subscribe({
      next: () => {
        this.toast.success(survey.userHasVoted ? 'Vote updated!' : 'Vote submitted!');
        this.submitting.set(false);
        this.loadSurvey(survey.id);
      },
      error: (err) => {
        this.toast.error(err.error?.error || 'Failed to submit vote');
        this.submitting.set(false);
      }
    });
  }

  getTimeRemaining(): string {
    const survey = this.survey();
    if (!survey) return '';

    const deadline = new Date(survey.deadline);
    const now = new Date();
    const diff = deadline.getTime() - now.getTime();

    if (diff <= 0) return 'Closed';

    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

    if (days > 0) return `${days}d ${hours}h remaining`;
    if (hours > 0) return `${hours}h ${minutes}m remaining`;
    return `${minutes}m remaining`;
  }

  getVotePercentage(voteCount: number): number {
    const survey = this.survey();
    if (!survey || survey.totalVotes === 0) return 0;
    return (voteCount / survey.totalVotes) * 100;
  }
}
