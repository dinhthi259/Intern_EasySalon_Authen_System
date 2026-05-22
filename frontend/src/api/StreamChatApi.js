import api from "./AxiosClient";

export const getStreamChatToken = (userId) => {
  return api.get(`/stream-chat/token/${userId}`);
};

export const getAdminStreamChatToken = () => {
  return api.get("/stream-chat/admin-token");
};