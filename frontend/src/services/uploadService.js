import API_URL from "./api";

const PUBLIC_BASE_URL = API_URL.replace(/\/api$/, "");

async function uploadFile(file, endpoint, errorMessage) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`${API_URL}/upload/${endpoint}`, {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    throw new Error(errorMessage);
  }

  const fileName = await response.text();
  return fileName.replace(/^"+|"+$/g, "");
}

export async function uploadImage(file) {
  const fileName = await uploadFile(file, "image", "Không tải ảnh lên được.");
  return `${PUBLIC_BASE_URL}/images/${fileName}`;
}

export async function uploadImages(files) {
  return await Promise.all(Array.from(files).map((file) => uploadImage(file)));
}

export async function uploadAudio(file) {
  return await uploadFile(file, "audio", "Không tải audio lên được.");
}
