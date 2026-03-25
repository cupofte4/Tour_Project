const LANGUAGES = [
  { code: "vi-VN", label: "🇻🇳 Tiếng Việt" },
  { code: "en-US", label: "🇺🇸 English" },
  { code: "zh-CN", label: "🇨🇳 中文" },
  { code: "de-DE", label: "🇩🇪 Deutsch" },
];

const LANG_FIELD = {
  "vi-VN": "textVi",
  "en-US": "textEn",
  "zh-CN": "textZh",
  "de-DE": "textDe",
};

const LANG_RATE = {
  "vi-VN": 0.98,
  "en-US": 0.94,
  "zh-CN": 0.92,
  "de-DE": 0.88,
};

const LANG_PITCH = {
  "vi-VN": 1,
  "en-US": 1.02,
  "zh-CN": 1,
  "de-DE": 1,
};

const PREFERRED_VOICE_NAMES = {
  "en-US": [
    "Microsoft Aria",
    "Microsoft Jenny",
    "Microsoft Guy",
    "Google US English",
    "Samantha",
    "Karen",
    "Daniel",
    "Moira",
    "Alex",
  ],
};

let cachedVoices = [];
let currentSessionId = 0;

const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const normalizeText = (text) =>
  (text || "").replace(/\s+/g, " ").replace(/\n/g, " ").trim();

function refreshVoices() {
  if (!window?.speechSynthesis) return [];
  cachedVoices = window.speechSynthesis.getVoices();
  return cachedVoices;
}

if (typeof window !== "undefined" && window.speechSynthesis) {
  window.speechSynthesis.onvoiceschanged = refreshVoices;
  refreshVoices();
}

async function ensureVoices() {
  const voices = refreshVoices();
  if (voices.length > 0) return voices;

  await wait(120);
  return refreshVoices();
}

function getVoiceForLang(langCode) {
  const voices = cachedVoices.length > 0 ? cachedVoices : refreshVoices();
  if (voices.length === 0) return null;

  const preferredNames = PREFERRED_VOICE_NAMES[langCode] || [];
  const preferredVoice = preferredNames
    .map((name) =>
      voices.find(
        (voice) =>
          voice.lang === langCode &&
          voice.name.toLowerCase().includes(name.toLowerCase())
      )
    )
    .find(Boolean);

  if (preferredVoice) {
    return preferredVoice;
  }

  const localVoice = voices.find(
    (voice) => voice.lang === langCode && voice.localService
  );
  if (localVoice) return localVoice;

  const exactVoice = voices.find((voice) => voice.lang === langCode);
  if (exactVoice) return exactVoice;

  const langPrefix = langCode.split("-")[0];
  return (
    voices.find((voice) => voice.lang.startsWith(`${langPrefix}-`)) ||
    voices.find((voice) => voice.lang.startsWith(langPrefix)) ||
    null
  );
}

function splitTextIntoChunks(text, langCode) {
  const normalized = normalizeText(text);
  if (!normalized) return [];

  const maxLength = langCode === "zh-CN" ? 80 : 180;
  const sentences =
    normalized.match(/[^.!?。！？;；]+[.!?。！？;；]?/g)?.map((item) => item.trim()) || [normalized];

  const chunks = [];
  let currentChunk = "";

  for (const sentence of sentences) {
    if (!sentence) continue;

    if ((currentChunk + " " + sentence).trim().length <= maxLength) {
      currentChunk = `${currentChunk} ${sentence}`.trim();
      continue;
    }

    if (currentChunk) {
      chunks.push(currentChunk);
      currentChunk = "";
    }

    if (sentence.length <= maxLength) {
      currentChunk = sentence;
      continue;
    }

    const words = sentence.split(" ");
    let partial = "";

    for (const word of words) {
      if ((partial + " " + word).trim().length <= maxLength) {
        partial = `${partial} ${word}`.trim();
      } else {
        if (partial) chunks.push(partial);
        partial = word;
      }
    }

    currentChunk = partial;
  }

  if (currentChunk) {
    chunks.push(currentChunk);
  }

  return chunks;
}

function getTextForLang(location, langCode) {
  const field = LANG_FIELD[langCode];
  return (location && location[field]) || location?.description || "";
}

function stop() {
  currentSessionId += 1;
  if (window?.speechSynthesis) {
    window.speechSynthesis.cancel();
  }
}

async function playChunks(text, langCode) {
  if (!window?.speechSynthesis) return;

  const chunks = splitTextIntoChunks(text, langCode);
  if (chunks.length === 0) return;

  const sessionId = currentSessionId + 1;
  currentSessionId = sessionId;

  await ensureVoices();
  window.speechSynthesis.cancel();
  await wait(120);

  for (const chunk of chunks) {
    if (sessionId !== currentSessionId) return;

    await new Promise((resolve) => {
      const utterance = new SpeechSynthesisUtterance(chunk);
      utterance.lang = langCode;
      utterance.rate = LANG_RATE[langCode] || 0.95;
      utterance.pitch = LANG_PITCH[langCode] || 1;
      utterance.volume = 1;

      const voice = getVoiceForLang(langCode);
      if (voice) {
        utterance.voice = voice;
      }

      utterance.onend = () => resolve();
      utterance.onerror = () => resolve();

      window.speechSynthesis.speak(utterance);
    });

    await wait(40);
  }
}

async function speak(text, langCode) {
  const normalized = normalizeText(text);
  if (!normalized) return;
  await playChunks(normalized, langCode);
}

function speakLocation(location, langCode) {
  const text = getTextForLang(location, langCode);
  return speak(text, langCode);
}

async function speakLocationAsync(location, langCode) {
  const text = getTextForLang(location, langCode);
  if (!text) return;
  await playChunks(text, langCode);
}

export { speak, speakLocation, speakLocationAsync, stop, LANGUAGES, getTextForLang };
