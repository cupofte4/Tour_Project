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

function getTextForLang(location, langCode) {
  const field = LANG_FIELD[langCode];
  return (location && location[field]) || location?.description || "";
}

let cachedVoices = [];
window.speechSynthesis.onvoiceschanged = () => {
  cachedVoices = window.speechSynthesis.getVoices();
};
cachedVoices = window.speechSynthesis.getVoices();

function speak(text, langCode) {
  if (!window.speechSynthesis || !text) return;
  window.speechSynthesis.cancel();

  const utter = new SpeechSynthesisUtterance(text);
  utter.lang = langCode;
  utter.rate = 0.95;
  const voice = cachedVoices.find(v => v.lang === langCode) ||
                cachedVoices.find(v => v.lang.startsWith(langCode.split('-')[0]));
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
    const utter = new SpeechSynthesisUtterance(text);
    utter.lang = langCode;
    utter.rate = 0.95;
    const voice = cachedVoices.find(v => v.lang === langCode) ||
                  cachedVoices.find(v => v.lang.startsWith(langCode.split('-')[0]));
    if (voice) utter.voice = voice;
    utter.onend = resolve;
    utter.onerror = resolve;
    window.speechSynthesis.speak(utter);
  });
}

function stop() {
  if (window.speechSynthesis) window.speechSynthesis.cancel();
}

export { speak, speakLocation, speakLocationAsync, stop, LANGUAGES, getTextForLang };
