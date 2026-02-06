import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { SelectionMode, OptionType, VoterSummary } from '../../models/survey.models';

interface EditorOption {
  id: string | null;
  textValue: string;
  dateValue: string;
}

@Component({
  selector: 'app-survey-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './survey-editor.component.html',
  styleUrl: './survey-editor.component.scss'
})
export class SurveyEditorComponent implements OnInit {
  isEditMode = signal(false);
  surveyId = signal<string | null>(null);
  loading = signal(false);
  saving = signal(false);
  voters = signal<VoterSummary[]>([]);

  title = '';
  description = '';
  selectionMode: SelectionMode = 'Single';
  optionType: OptionType = 'Text';
  deadline = '';
  options: EditorOption[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.surveyId.set(id);
      this.loadSurvey(id);
    } else {
      this.addOption();
      this.addOption();
      this.setDefaultDeadline();
    }
  }

  setDefaultDeadline(): void {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 7);
    this.deadline = this.formatDateTimeForInput(tomorrow);
  }

  formatDateTimeForInput(date: Date): string {
    // Format as local time for datetime-local input (YYYY-MM-DDTHH:mm)
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  formatDateForInput(date: Date): string {
    // Format as local date for date input (YYYY-MM-DD)
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  loadSurvey(id: string): void {
    this.loading.set(true);
    this.api.getAdminSurvey(id).subscribe({
      next: (survey) => {
        this.title = survey.title;
        this.description = survey.description || '';
        this.selectionMode = survey.selectionMode;
        this.optionType = survey.optionType;
        this.deadline = this.formatDateTimeForInput(new Date(survey.deadline));
        this.options = survey.options.map(o => ({
          id: o.id,
          textValue: o.textValue || '',
          dateValue: o.dateValue ? this.formatDateForInput(new Date(o.dateValue)) : ''
        }));
        this.loading.set(false);
        this.loadVoters(id);
      },
      error: () => {
        this.toast.error('Failed to load survey');
        this.loading.set(false);
        this.router.navigate(['/admin']);
      }
    });
  }

  loadVoters(id: string): void {
    this.api.getSurveyVoters(id).subscribe({
      next: (response) => {
        this.voters.set(response.voters);
      }
    });
  }

  formatVotedAt(dateString: string): string {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  addOption(): void {
    this.options.push({
      id: null,
      textValue: '',
      dateValue: ''
    });
  }

  removeOption(index: number): void {
    if (this.options.length > 2) {
      this.options.splice(index, 1);
    }
  }

  moveOption(index: number, direction: 'up' | 'down'): void {
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex >= 0 && newIndex < this.options.length) {
      const temp = this.options[index];
      this.options[index] = this.options[newIndex];
      this.options[newIndex] = temp;
    }
  }

  validate(): string | null {
    if (!this.title.trim()) return 'Title is required';
    if (!this.deadline) return 'Deadline is required';
    if (this.options.length < 2) return 'At least 2 options are required';

    for (let i = 0; i < this.options.length; i++) {
      const opt = this.options[i];
      if (this.optionType === 'Text' && !opt.textValue.trim()) {
        return `Option ${i + 1} text is required`;
      }
      if (this.optionType === 'Date' && !opt.dateValue) {
        return `Option ${i + 1} date is required`;
      }
    }

    return null;
  }

  save(): void {
    const validationError = this.validate();
    if (validationError) {
      this.toast.error(validationError);
      return;
    }

    this.saving.set(true);

    const data = {
      title: this.title.trim(),
      description: this.description.trim() || null,
      selectionMode: this.selectionMode,
      optionType: this.optionType,
      deadline: new Date(this.deadline).toISOString(),
      options: this.options.map(o => ({
        id: o.id,
        textValue: this.optionType === 'Text' ? o.textValue.trim() : null,
        dateValue: this.optionType === 'Date' ? o.dateValue : null
      }))
    };

    if (this.isEditMode()) {
      this.api.updateSurvey(this.surveyId()!, data).subscribe({
        next: () => {
          this.toast.success('Survey updated');
          this.router.navigate(['/admin']);
        },
        error: (err) => {
          this.toast.error(err.error?.error || 'Failed to update survey');
          this.saving.set(false);
        }
      });
    } else {
      this.api.createSurvey(data).subscribe({
        next: () => {
          this.toast.success('Survey created');
          this.router.navigate(['/admin']);
        },
        error: (err) => {
          this.toast.error(err.error?.error || 'Failed to create survey');
          this.saving.set(false);
        }
      });
    }
  }
}
