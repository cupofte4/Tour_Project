import { useEffect, useRef, useState } from "react";
import { Html5Qrcode } from "html5-qrcode";

const QR_ELEMENT_ID = "qr-reader-container";

/**
 * QRScanner component
 *
 * QR code format expected: JSON string  →  { "locationId": 3 }
 * OR plain number string                →  "3"
 *
 * Props:
 *   onDetected(locationId: number) — called once per scan session
 *   onClose()                      — called when user closes scanner
 */
function QRScanner({ onDetected, onClose }) {
  const [error, setError] = useState(null);
  const [scanning, setScanning] = useState(false);
  const scannerRef = useRef(null);
  const detectedRef = useRef(false); // prevent double-fire

  useEffect(() => {
    const scanner = new Html5Qrcode(QR_ELEMENT_ID);
    scannerRef.current = scanner;
    detectedRef.current = false;

    scanner
      .start(
        { facingMode: "environment" }, // rear camera
        { fps: 10, qrbox: { width: 240, height: 240 } },
        (decodedText) => {
          if (detectedRef.current) return;

          const locationId = parseLocationId(decodedText);
          if (!locationId) {
            setError(`QR không hợp lệ: "${decodedText}"`);
            return;
          }

          detectedRef.current = true;
          stopScanner(scanner);
          onDetected(locationId);
        },
        () => {} // ignore per-frame errors
      )
      .then(() => setScanning(true))
      .catch((err) => {
        setError(
          err?.message?.includes("Permission")
            ? "Bạn chưa cấp quyền camera. Vui lòng cho phép trong cài đặt trình duyệt."
            : `Không thể mở camera: ${err?.message ?? err}`
        );
      });

    return () => stopScanner(scanner);
  }, [onDetected]);

  function stopScanner(scanner) {
    scanner
      .stop()
      .catch(() => {})
      .finally(() => scanner.clear().catch(() => {}));
  }

  function parseLocationId(text) {
    const trimmed = (text || "").trim();

    // Plain number: "5"
    const asNumber = Number(trimmed);
    if (Number.isInteger(asNumber) && asNumber > 0) return asNumber;

    // JSON: { "locationId": 5 }  or  { "id": 5 }
    try {
      const parsed = JSON.parse(trimmed);
      const id = parsed?.locationId ?? parsed?.id ?? parsed?.LocationId;
      if (id && Number.isInteger(Number(id))) return Number(id);
    } catch {
      // not JSON
    }

    return null;
  }

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.header}>
          <span>📷 Quét mã QR</span>
          <button style={styles.closeBtn} onClick={onClose}>✕</button>
        </div>

        <p style={styles.hint}>
          Hướng camera vào mã QR tại địa điểm để nghe thuyết minh tự động.
        </p>

        {/* Camera viewport */}
        <div style={styles.viewportWrapper}>
          <div id={QR_ELEMENT_ID} style={styles.viewport} />
          {scanning && !error && (
            <div style={styles.scanFrame}>
              <div style={styles.corner("topLeft")} />
              <div style={styles.corner("topRight")} />
              <div style={styles.corner("bottomLeft")} />
              <div style={styles.corner("bottomRight")} />
            </div>
          )}
        </div>

        {error && (
          <div style={styles.errorBox}>
            <span>⚠️ {error}</span>
          </div>
        )}

        {!scanning && !error && (
          <p style={{ textAlign: "center", color: "#888", fontSize: 13 }}>
            ⏳ Đang khởi động camera...
          </p>
        )}

        <button style={styles.cancelBtn} onClick={onClose}>
          Đóng
        </button>
      </div>
    </div>
  );
}

// ── Inline styles ──────────────────────────────────────────
const CORNER_SIZE = 18;
const CORNER_THICKNESS = 3;
const CORNER_COLOR = "#2196F3";

const cornerBase = {
  position: "absolute",
  width: CORNER_SIZE,
  height: CORNER_SIZE,
  borderColor: CORNER_COLOR,
  borderStyle: "solid",
};

const styles = {
  overlay: {
    position: "fixed", inset: 0,
    background: "rgba(0,0,0,0.75)",
    zIndex: 9999,
    display: "flex", alignItems: "center", justifyContent: "center",
  },
  modal: {
    background: "#fff",
    borderRadius: 16,
    padding: "20px 20px 16px",
    width: "min(92vw, 380px)",
    boxShadow: "0 8px 32px rgba(0,0,0,0.3)",
    display: "flex", flexDirection: "column", gap: 12,
  },
  header: {
    display: "flex", justifyContent: "space-between", alignItems: "center",
    fontWeight: 700, fontSize: 16,
  },
  closeBtn: {
    background: "none", border: "none", fontSize: 18,
    cursor: "pointer", color: "#666", lineHeight: 1,
  },
  hint: {
    fontSize: 13, color: "#555", margin: 0, textAlign: "center",
  },
  viewportWrapper: {
    position: "relative",
    borderRadius: 12, overflow: "hidden",
    background: "#000",
    minHeight: 260,
  },
  viewport: {
    width: "100%",
  },
  scanFrame: {
    position: "absolute",
    top: "50%", left: "50%",
    transform: "translate(-50%, -50%)",
    width: 200, height: 200,
    pointerEvents: "none",
  },
  corner: (pos) => {
    const map = {
      topLeft:     { top: 0, left: 0,  borderWidth: `${CORNER_THICKNESS}px 0 0 ${CORNER_THICKNESS}px` },
      topRight:    { top: 0, right: 0, borderWidth: `${CORNER_THICKNESS}px ${CORNER_THICKNESS}px 0 0` },
      bottomLeft:  { bottom: 0, left: 0,  borderWidth: `0 0 ${CORNER_THICKNESS}px ${CORNER_THICKNESS}px` },
      bottomRight: { bottom: 0, right: 0, borderWidth: `0 ${CORNER_THICKNESS}px ${CORNER_THICKNESS}px 0` },
    };
    return { ...cornerBase, ...map[pos] };
  },
  errorBox: {
    background: "#fff3cd", border: "1px solid #ffc107",
    borderRadius: 8, padding: "10px 14px",
    fontSize: 13, color: "#856404",
  },
  cancelBtn: {
    padding: "10px 0", background: "#f5f5f5",
    border: "1px solid #ddd", borderRadius: 8,
    cursor: "pointer", fontSize: 14, fontWeight: 600,
    color: "#333",
  },
};

export default QRScanner;
