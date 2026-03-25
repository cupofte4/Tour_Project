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

// Rate configuration for different languages
const LANG_RATE = {
  "vi-VN": 0.95,
  "en-US": 0.95,
  "zh-CN": 0.95,
  "de-DE": 0.80, // German - slower due to complex phonetics
};

function getTextForLang(location, langCode) {
  const field = LANG_FIELD[langCode];
  return (location && location[field]) || location?.description || "";
}

let cachedVoices = [];
window.speechSynthesis.onvoiceschanged = () => {
  cachedVoices = window.speechSynthesis.getVoices();
};
cachedVoices = window.speechSynthesis.getVoices();

function getVoiceForLang(langCode) {
  // Try exact match first
  let voice = cachedVoices.find(v => v.lang === langCode);
  if (voice) return voice;
  
  // Try language code prefix match (e.g., "de" for "de-DE")
  const langPrefix = langCode.split('-')[0];
  voice = cachedVoices.find(v => v.lang.startsWith(langPrefix + '-'));
  if (voice) return voice;
  
  // For German, try common variations
  if (langCode === 'de-DE') {
    voice = cachedVoices.find(v => v.lang.includes('de') && v.lang.includes('DE'));
    if (voice) return voice;
  }
  
  // Fallback - just find any voice with matching language prefix
  voice = cachedVoices.find(v => v.lang.startsWith(langPrefix));
  return voice || null;
}

// Split text into sentences for better speech synthesis handling
function splitTextIntoSentences(text, langCode) {
  if (!text) return [];
  
  // For German and longer texts, split more aggressively
  if (langCode === 'de-DE') {
    // Split by sentence boundaries and limit chunk size
    const sentences = text.match(/[^.!?]+[.!?]+/g) || [text];
    const chunks = [];
    let currentChunk = '';
    
    for (const sentence of sentences) {
      const trimmed = sentence.trim();
      if (currentChunk.length + trimmed.length > 300) {
        if (currentChunk) chunks.push(currentChunk);
        currentChunk = trimmed;
      } else {
        currentChunk += ' ' + trimmed;
      }
    }
    if (currentChunk) chunks.push(currentChunk);
    
    return chunks.map(c => c.trim()).filter(c => c.length > 0);
  }
  
  // For other languages, use normal chunking
  return [text];
}

function speak(text, langCode) {
  if (!window.speechSynthesis || !text) return;
  window.speechSynthesis.cancel();

  const utter = new SpeechSynthesisUtterance(text);
  utter.lang = langCode;
  utter.rate = LANG_RATE[langCode] || 0.95;
  utter.pitch = 1.0;
  utter.volume = 1.0;
  const voice = getVoiceForLang(langCode);
  if (voice) utter.voice = voice;
  window.speechSynthesis.speak(utter);
}

function speakLocation(location, langCode) {
  const field = LANG_FIELD[langCode];
  const text = (location && location[field]) || location?.description || "";
  if (text) speak(text, langCode);
}

function speakLocationAsync(location, langCode) {
  return new Promise((resolve) => {
    const field = LANG_FIELD[langCode];
    const text = (location && location[field]) || location?.description || "";
    if (!text) return resolve();

    window.speechSynthesis.cancel();
    
    // Split text into chunks for better speech synthesis
    const chunks = splitTextIntoSentences(text, langCode);
    let currentChunkIndex = 0;

    const speakChunk = () => {
      if (currentChunkIndex >= chunks.length) {
        return resolve();
      }

      const chunk = chunks[currentChunkIndex];
      const utter = new SpeechSynthesisUtterance(chunk);
      utter.lang = langCode;
      utter.rate = LANG_RATE[langCode] || 0.95;
      utter.pitch = 1.0;
      utter.volume = 1.0;
      
      const voice = getVoiceForLang(langCode);
      if (voice) utter.voice = voice;

      utter.onend = () => {
        currentChunkIndex++;
        // Small pause between chunks for German
        const pauseMs = langCode === 'de-DE' ? 100 : 50;
        setTimeout(speakChunk, pauseMs);
      };

      utter.onerror = (error) => {
        console.error('Speech synthesis error:', error);
        currentChunkIndex++;
        setTimeout(speakChunk, 100);
      };

      window.speechSynthesis.speak(utter);
    };

    speakChunk();
  });
}

function stop() {
  if (window.speechSynthesis) window.speechSynthesis.cancel();
}

export { speak, speakLocation, speakLocationAsync, stop, LANGUAGES, getTextForLang };
