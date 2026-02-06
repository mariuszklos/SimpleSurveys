export type SelectionMode = 'Single' | 'Multiple';
export type OptionType = 'Text' | 'Date';

export interface SurveyOption {
  id: string;
  textValue: string | null;
  dateValue: string | null;
  displayText: string;
  voteCount: number;
  isWinner: boolean;
}

export interface Survey {
  id: string;
  title: string;
  description: string | null;
  selectionMode: SelectionMode;
  optionType: OptionType;
  deadline: string;
  isActive: boolean;
  options: SurveyOption[];
  totalVotes: number;
  userHasVoted: boolean;
  currentVoterName: string | null;
}

export interface SurveyListItem {
  id: string;
  title: string;
  selectionMode: SelectionMode;
  optionType: OptionType;
  deadline: string;
  isActive: boolean;
  totalVotes: number;
  createdAt: string;
}

export interface CreateSurveyRequest {
  title: string;
  description: string | null;
  selectionMode: SelectionMode;
  optionType: OptionType;
  deadline: string;
  options: CreateOptionRequest[];
}

export interface CreateOptionRequest {
  textValue: string | null;
  dateValue: string | null;
}

export interface UpdateSurveyRequest {
  title: string;
  description: string | null;
  selectionMode: SelectionMode;
  optionType: OptionType;
  deadline: string;
  options: UpdateOptionRequest[];
}

export interface UpdateOptionRequest {
  id: string | null;
  textValue: string | null;
  dateValue: string | null;
}

export interface VoteRequest {
  optionIds: string[];
  voterName: string | null;
}

export interface MyVotesResponse {
  optionIds: string[];
  voterName: string | null;
}

export interface VoterSummary {
  voterName: string;
  selectedOptions: string[];
  votedAt: string;
}

export interface SurveyVotersResponse {
  surveyId: string;
  voters: VoterSummary[];
}
