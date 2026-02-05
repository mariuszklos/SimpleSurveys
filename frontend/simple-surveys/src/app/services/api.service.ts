import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Survey,
  SurveyListItem,
  CreateSurveyRequest,
  UpdateSurveyRequest,
  VoteRequest,
  MyVotesResponse,
  SurveyVotersResponse
} from '../models/survey.models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5226/api';

  constructor(private http: HttpClient) {}

  // Public endpoints
  getSurvey(id: string): Observable<Survey> {
    return this.http.get<Survey>(`${this.baseUrl}/surveys/${id}`);
  }

  vote(surveyId: string, request: VoteRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/surveys/${surveyId}/vote`, request);
  }

  getMyVotes(surveyId: string): Observable<MyVotesResponse> {
    return this.http.get<MyVotesResponse>(`${this.baseUrl}/surveys/${surveyId}/my-votes`);
  }

  // Admin endpoints
  adminLogin(password: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/admin/login`, { password });
  }

  adminLogout(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/admin/logout`, {});
  }

  checkAdminAuth(): Observable<{ authenticated: boolean }> {
    return this.http.get<{ authenticated: boolean }>(`${this.baseUrl}/admin/check`);
  }

  getAdminSurveys(): Observable<SurveyListItem[]> {
    return this.http.get<SurveyListItem[]>(`${this.baseUrl}/admin/surveys`);
  }

  getAdminSurvey(id: string): Observable<Survey> {
    return this.http.get<Survey>(`${this.baseUrl}/admin/surveys/${id}`);
  }

  createSurvey(request: CreateSurveyRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.baseUrl}/admin/surveys`, request);
  }

  updateSurvey(id: string, request: UpdateSurveyRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/admin/surveys/${id}`, request);
  }

  deleteSurvey(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/admin/surveys/${id}`);
  }

  getSurveyVoters(id: string): Observable<SurveyVotersResponse> {
    return this.http.get<SurveyVotersResponse>(`${this.baseUrl}/admin/surveys/${id}/voters`);
  }
}
