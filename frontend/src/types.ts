export interface ChatMessage {
  role: "assistant" | "user";
  content: string;
  timestamp: string;
}

export interface CvProfile {
  fullName: string;
  targetRole: string;
  email: string;
  phone: string;
  location: string;
  linkedIn?: string;
  summary: string;
  skills: string[];
  experience: string[];
  education: string[];
  certifications: string[];
  projects: string[];
  languages: string[];
  achievements: string[];
  additionalInformation: string[];
}

export interface ServerCvProfile extends CvProfile {
  certificationLabels?: string[];
  projectSummaries?: string[];
  languageLabels?: string[];
}

export interface Workspace {
  profile: ServerCvProfile;
  messages: ChatMessage[];
  currentStep: string;
  selectedTemplateId: string;
  uploadedDocumentName: string;
  sourceText: string;
  aiStatus: string;
  aiError: string;
  aiNextQuestion: string;
  aiTemplateRecommendation: string;
  aiMissingFields: string[];
  aiSuggestedImprovements: string[];
}

export interface ResumeAnalysisItem {
  label: string;
  found: boolean;
}

export interface UploadedDocument {
  fileName: string;
  extractedText: string;
  profile?: CvProfile;
  aiError?: string;
  status?: "success" | "partial";
  message?: string;
  fileSizeBytes?: number;
  contentType?: string;
  extractedCharacterCount?: number;
}

export interface SpeechRecognitionResultEvent extends Event {
  results: ArrayLike<ArrayLike<{ transcript: string }>>;
}

export interface SpeechRecognitionInstance {
  lang: string;
  continuous: boolean;
  interimResults: boolean;
  onresult: ((event: SpeechRecognitionResultEvent) => void) | null;
  onerror: (() => void) | null;
  onend: (() => void) | null;
  start(): void;
  stop(): void;
}

declare global {
  interface Window {
    SpeechRecognition?: new () => SpeechRecognitionInstance;
    webkitSpeechRecognition?: new () => SpeechRecognitionInstance;
  }
}
