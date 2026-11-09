import { useEffect, useRef, useState } from "react";
import {
  Button,
  Card,
  Text,
  Textarea,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { Mic20Regular, Send20Regular, Stop20Regular } from "@fluentui/react-icons";
import type { ChatMessage, CvProfile, SpeechRecognitionInstance } from "../types";
import { hasProfileContent, hasProfileItems } from "../utils/resumeParser";

const useStyles = makeStyles({
  card: {
    display: "flex",
    flexDirection: "column",
    minHeight: "520px",
    height: "100%",
    padding: "20px",
  },
  messages: {
    display: "flex",
    flex: 1,
    flexDirection: "column",
    gap: "14px",
    overflowY: "auto",
    padding: "18px 2px",
  },
  message: { display: "flex", gap: "10px", alignItems: "flex-start" },
  bubble: {
    maxWidth: "82%",
    padding: "11px 13px",
    borderRadius: tokens.borderRadiusLarge,
    backgroundColor: tokens.colorNeutralBackground2,
    whiteSpace: "pre-wrap",
  },
  userBubble: {
    backgroundColor: tokens.colorBrandBackground2,
  },
  composer: { display: "grid", gap: "10px" },
  controls: { display: "flex", justifyContent: "space-between", alignItems: "center" },
});

interface ConversationPanelProps {
  messages: ChatMessage[];
  disabled: boolean;
  profile: CvProfile;
  onSend: (message: string) => Promise<void>;
  onSaveProfile: (profile: CvProfile) => Promise<boolean>;
  onError: (message: string) => void;
}

export default function ConversationPanel({
  messages,
  disabled,
  profile,
  onSend,
  onSaveProfile,
  onError,
}: ConversationPanelProps) {
  const styles = useStyles();
  const [draft, setDraft] = useState("");
  const [listening, setListening] = useState(false);
  const [sending, setSending] = useState(false);
  const [guidedBuildStarted, setGuidedBuildStarted] = useState(false);
  const [guidedQuestions, setGuidedQuestions] = useState<GuidedQuestion[]>([]);
  const [guidedQuestionIndex, setGuidedQuestionIndex] = useState(0);
  const [guidedMessages, setGuidedMessages] = useState<ChatMessage[]>([]);
  const messagesRef = useRef<HTMLDivElement>(null);
  const recognitionRef = useRef<SpeechRecognitionInstance | null>(null);

  useEffect(() => {
    messagesRef.current?.scrollTo({ top: messagesRef.current.scrollHeight, behavior: "smooth" });
  }, [messages]);

  async function handleSend() {
    const message = draft.trim();
    if (!message || sending || disabled) return;
    setSending(true);
    onError("");
    try {
      if (guidedBuildStarted && guidedQuestions[guidedQuestionIndex]) {
        const question = guidedQuestions[guidedQuestionIndex];
        const updatedProfile = applyGuidedAnswer(profile, question.field, message);
        if (!await onSaveProfile(updatedProfile)) return;

        const nextIndex = guidedQuestionIndex + 1;
        const answerMessages: ChatMessage[] = [
          { role: "user", content: message, timestamp: new Date().toISOString() },
        ];
        if (nextIndex < guidedQuestions.length) {
          answerMessages.push({
            role: "assistant",
            content: guidedQuestions[nextIndex].prompt,
            timestamp: new Date().toISOString(),
          });
        } else {
          answerMessages.push({
            role: "assistant",
            content: "Thank you. Your answers have been added to the CV preview. Review the details before exporting.",
            timestamp: new Date().toISOString(),
          });
          setGuidedBuildStarted(false);
        }
        setGuidedMessages((current) => [...current, ...answerMessages]);
        setGuidedQuestionIndex(nextIndex);
        setDraft("");
        return;
      }

      setGuidedMessages([]);
      await onSend(message);
      setDraft("");
    } catch {
      onError("Your CV details could not be saved. Please try again.");
    } finally {
      setSending(false);
    }
  }

  function toggleVoiceInput() {
    if (listening) {
      recognitionRef.current?.stop();
      return;
    }

    const SpeechRecognition = window.SpeechRecognition ?? window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
      onError("Voice input is unavailable in this browser.");
      return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = navigator.language || "en-US";
    recognition.continuous = true;
    recognition.interimResults = true;
    recognition.onresult = (event) => {
      const transcript = Array.from(event.results)
        .map((result) => result[0]?.transcript ?? "")
        .join("");
      setDraft(transcript);
    };
    recognition.onerror = () => {
      setListening(false);
      onError("Voice input stopped. Please try again.");
    };
    recognition.onend = () => setListening(false);
    recognitionRef.current = recognition;
    setListening(true);
    onError("");
    recognition.start();
  }

  const visibleMessages = guidedMessages.length
    ? guidedMessages
    : messages.filter((message) => message.role === "user");

  function startGuidedBuild() {
    const questions = getGuidedQuestions(profile);
    setGuidedMessages([]);
    setGuidedQuestions(questions);
    setGuidedQuestionIndex(0);
    if (questions.length === 0) {
      setGuidedMessages([{
        role: "assistant",
        content: "The available CV sections are complete. You can still add corrections or extra details below.",
        timestamp: new Date().toISOString(),
      }]);
      setGuidedBuildStarted(false);
      return;
    }
    setGuidedBuildStarted(true);
    setGuidedMessages([{
      role: "assistant",
      content: questions[0].prompt,
      timestamp: new Date().toISOString(),
    }]);
  }

  return (
    <Card className={styles.card}>
      <div className="panel-heading">
        <div>
          <Text weight="semibold" size={400}>Additional CV Details</Text>
          <Text className="muted-text" size={200}>Add information or corrections for your CV.</Text>
        </div>
        <Button
          appearance="secondary"
          size="small"
          disabled={disabled || sending || guidedBuildStarted}
          onClick={startGuidedBuild}
        >
          {guidedBuildStarted ? "Q&A in progress" : "Start CV Q&A"}
        </Button>
      </div>
      <div ref={messagesRef} className={styles.messages} aria-live="polite">
        {visibleMessages.length ? visibleMessages.map((message, index) => (
          <div className={styles.message} key={`${message.timestamp}-${index}`}>
            <div className={`${styles.bubble} ${message.role === "user" ? styles.userBubble : ""}`}>
              <Text size={300}>{message.content}</Text>
            </div>
          </div>
        )) : (
          <Text className="muted-text" size={200}>
            Start a guided CV Q&A or add information and corrections below.
          </Text>
        )}
      </div>
      {guidedBuildStarted && (
        <Text className="muted-text voice-guidance-note" size={100}>
          Answer by typing or use voice input. Voice answers are added as text for your review.
        </Text>
      )}
      <div className={styles.composer}>
        <Textarea
          resize="vertical"
          rows={3}
          placeholder="Add a detail or correction…"
          value={draft}
          disabled={disabled || sending}
          onChange={(_, data) => setDraft(data.value)}
          onKeyDown={(event) => {
            if (event.key === "Enter" && !event.shiftKey) {
              event.preventDefault();
              void handleSend();
            }
          }}
        />
        <div className={styles.controls}>
          <Button
            appearance={listening ? "primary" : "subtle"}
            icon={listening ? <Stop20Regular /> : <Mic20Regular />}
            onClick={toggleVoiceInput}
            aria-label={listening ? "Stop voice input" : "Start voice input"}
          >
            {listening ? "Stop voice input" : "Voice input"}
          </Button>
          <Button
            appearance="primary"
            icon={<Send20Regular />}
            disabled={disabled || sending || !draft.trim()}
            onClick={() => void handleSend()}
          >
            {sending ? "Saving…" : "Add details"}
          </Button>
        </div>
      </div>
    </Card>
  );
}

type GuidedField =
  | "fullName"
  | "contact"
  | "summary"
  | "role"
  | "skills"
  | "experience"
  | "achievements"
  | "project"
  | "projectContribution"
  | "certifications"
  | "education"
  | "educationInstitution"
  | "languages"
  | "additionalInformation";

interface GuidedQuestion {
  field: GuidedField;
  prompt: string;
}

function getGuidedQuestions(profile: CvProfile): GuidedQuestion[] {
  const questions: GuidedQuestion[] = [];
  if (!hasProfileContent(profile.fullName)) {
    questions.push({ field: "fullName", prompt: "What is your full name?" });
  }
  if (!hasProfileContent(profile.email) && !hasProfileContent(profile.phone)) {
    questions.push({ field: "contact", prompt: "What email address and phone number would you like displayed on your CV?" });
  }
  if (!hasProfileContent(profile.summary)) {
    questions.push({ field: "summary", prompt: "Please introduce yourself in 2–3 sentences." });
  }
  if (!hasProfileContent(profile.targetRole) || !hasProfileItems(profile.experience)) {
    questions.push({ field: "role", prompt: "What is your current job title and total years of experience?" });
  }
  if (!hasProfileItems(profile.skills)) {
    questions.push({
      field: "skills",
      prompt: "What are your primary technical skills and tools? Which technologies, platforms, or frameworks do you use regularly?",
    });
  }
  if (!hasProfileItems(profile.experience)) {
    questions.push({ field: "experience", prompt: "Tell me about your current or most recent role and your key responsibilities." });
  }
  if (!hasProfileItems(profile.achievements)) {
    questions.push({ field: "achievements", prompt: "What are your top 2–3 professional achievements?" });
  }
  if (!hasProfileItems(profile.projects)) {
    questions.push(
      { field: "project", prompt: "Tell me about your most important project." },
      { field: "projectContribution", prompt: "What was your role and contribution in that project?" },
    );
  } else if (!profile.projects.some((project) => /\b(role|contribution|responsibilit|developed|designed|implemented)\b/i.test(project))) {
    questions.push({ field: "projectContribution", prompt: "What was your role and contribution in one of these projects?" });
  }
  if (!hasProfileItems(profile.certifications)) {
    questions.push({ field: "certifications", prompt: "Which certifications do you currently hold?" });
  }
  if (!hasProfileItems(profile.education)) {
    questions.push(
      { field: "education", prompt: "What is your highest educational qualification?" },
    );
  }
  if (!profile.education.some((item) => /\b(college|university|institute)\b/i.test(item))) {
    questions.push({ field: "educationInstitution", prompt: "Which college or university did you attend?" });
  }
  if (!hasProfileItems(profile.languages)) {
    questions.push({ field: "languages", prompt: "What languages do you speak or write professionally?" });
  }
  if (!hasProfileItems(profile.additionalInformation)) {
    questions.push({ field: "additionalInformation", prompt: "Do you have any awards, recognitions, publications, or other information to include? Is there anything else you would like to add to strengthen your CV?" });
  }
  return questions;
}

function applyGuidedAnswer(profile: CvProfile, field: GuidedField, answer: string): CvProfile {
  const cleanedAnswer = answer.trim();
  const append = (current: string[], values: string[]) => {
    const combined = [...current.filter(hasProfileContent), ...values.map((value) => value.trim()).filter(Boolean)];
    return combined.filter((item, index) =>
      combined.findIndex((other) => other.toLowerCase() === item.toLowerCase()) === index);
  };
  const splitItems = (value: string) => value
    .split(/[\r\n,;]+/)
    .map((item) => item.trim())
    .filter(Boolean);

  switch (field) {
    case "fullName":
      return { ...profile, fullName: cleanedAnswer };
    case "contact": {
      const email = cleanedAnswer.match(/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b/i)?.[0];
      const phone = cleanedAnswer.match(/(?:\+\d[\d ().-]{5,}\d|\(\d{3}\)\s*\d{3}[- .]\d{4}|\b\d{10,15}\b)/)?.[0];
      return {
        ...profile,
        email: email ?? profile.email,
        phone: phone ?? profile.phone,
      };
    }
    case "summary":
      return { ...profile, summary: cleanedAnswer };
    case "role": {
      const years = cleanedAnswer.match(/\b\d+(?:\.\d+)?\+?\s*(?:years?|yrs?)(?:\s+of\s+experience)?\b/i)?.[0];
      const role = cleanedAnswer
        .replace(/(?:[,;|]\s*|\s+with\s+)\d+(?:\.\d+)?\+?\s*(?:years?|yrs?)(?:\s+of\s+experience)?/i, "")
        .trim();
      return {
        ...profile,
        targetRole: role || cleanedAnswer,
        additionalInformation: years
          ? append(profile.additionalInformation, [`Total experience: ${years}`])
          : profile.additionalInformation,
      };
    }
    case "skills":
    case "languages":
    case "certifications":
      return { ...profile, [field]: append(profile[field], splitItems(cleanedAnswer)) };
    case "experience":
      return { ...profile, experience: append(profile.experience, [cleanedAnswer]) };
    case "achievements":
      return { ...profile, achievements: append(profile.achievements, splitItems(cleanedAnswer)) };
    case "project":
      return { ...profile, projects: append(profile.projects, [cleanedAnswer]) };
    case "projectContribution": {
      const projects = profile.projects.filter(hasProfileContent);
      if (projects.length === 0) return { ...profile, projects: [cleanedAnswer] };
      const lastProject = projects[projects.length - 1];
      projects[projects.length - 1] = `${lastProject} | Contribution: ${cleanedAnswer}`;
      return { ...profile, projects };
    }
    case "education":
      return { ...profile, education: append(profile.education, [cleanedAnswer]) };
    case "educationInstitution": {
      const education = profile.education.filter(hasProfileContent);
      if (education.length === 0) return { ...profile, education: [`Institution: ${cleanedAnswer}`] };
      const lastEntry = education[education.length - 1];
      education[education.length - 1] = `${lastEntry} | Institution: ${cleanedAnswer}`;
      return { ...profile, education };
    }
    case "additionalInformation":
      return { ...profile, additionalInformation: append(profile.additionalInformation, [cleanedAnswer]) };
  }
}
