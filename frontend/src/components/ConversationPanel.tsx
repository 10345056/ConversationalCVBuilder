import { useEffect, useRef, useState } from "react";
import {
  Avatar,
  Button,
  Card,
  Text,
  Textarea,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { Mic20Regular, Send20Regular, Stop20Regular } from "@fluentui/react-icons";
import type { ChatMessage, SpeechRecognitionInstance } from "../types";

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
  userMessage: { flexDirection: "row-reverse" },
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
  onSend: (message: string) => Promise<void>;
  onError: (message: string) => void;
}

export default function ConversationPanel({
  messages,
  disabled,
  onSend,
  onError,
}: ConversationPanelProps) {
  const styles = useStyles();
  const [draft, setDraft] = useState("");
  const [listening, setListening] = useState(false);
  const [sending, setSending] = useState(false);
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
      await onSend(message);
      setDraft("");
    } catch (error) {
      onError(error instanceof Error ? error.message : "Your message could not be sent.");
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
      onError("Voice input is not supported in this browser. Try a current version of Chrome or Edge.");
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
      onError("Voice input stopped. Check microphone permissions and try again.");
    };
    recognition.onend = () => setListening(false);
    recognitionRef.current = recognition;
    setListening(true);
    onError("");
    recognition.start();
  }

  return (
    <Card className={styles.card}>
      <div className="panel-heading">
        <div>
          <Text weight="semibold" size={400}>Build it together</Text>
          <Text className="muted-text" size={200}>Your guided CV conversation</Text>
        </div>
        <span className="online-indicator"><span /> Ready</span>
      </div>
      <div ref={messagesRef} className={styles.messages} aria-live="polite">
        {messages.map((message, index) => {
          const isUser = message.role === "user";
          return (
            <div className={`${styles.message} ${isUser ? styles.userMessage : ""}`} key={`${message.timestamp}-${index}`}>
              {!isUser && <Avatar name="CV assistant" color="brand" size={28} />}
              <div className={`${styles.bubble} ${isUser ? styles.userBubble : ""}`}>
                <Text size={300}>{message.content}</Text>
              </div>
            </div>
          );
        })}
      </div>
      <div className={styles.composer}>
        <Textarea
          resize="vertical"
          rows={3}
          placeholder="Write your answer…"
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
            aria-label={listening ? "Stop voice recording" : "Start voice recording"}
          >
            {listening ? "Stop recording" : "Record & transcribe"}
          </Button>
          <Button
            appearance="primary"
            icon={<Send20Regular />}
            disabled={disabled || sending || !draft.trim()}
            onClick={() => void handleSend()}
          >
            {sending ? "Sending…" : "Send"}
          </Button>
        </div>
      </div>
    </Card>
  );
}
