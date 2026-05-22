import { useEffect, useState } from "react";
import { StreamChat } from "stream-chat";
import {
  Chat,
  Channel,
  ChannelList,
  Window,
  ChannelHeader,
  MessageList,
  MessageComposer,
} from "stream-chat-react";
import { getAdminStreamChatToken } from "../../api/StreamChatApi";

function AdminChatPage() {
  const [client, setClient] = useState(null);
  const [activeChannel, setActiveChannel] = useState(null);

  useEffect(() => {
    let chatClient;
    let isMounted = true;

    const init = async () => {
      const res = await getAdminStreamChatToken();

      chatClient = StreamChat.getInstance(res.data.apiKey);

      await chatClient.connectUser(
        {
          id: String(res.data.userId),
          name: res.data.fullName || "Nhân viên tư vấn",
        },
        res.data.token
      );

      if (isMounted) {
        setClient(chatClient);
      }
    };

    init();

    return () => {
      isMounted = false;

      if (chatClient) {
        chatClient.disconnectUser();
      }
    };
  }, []);

  if (!client) {
    return <p>Đang tải hộp thư...</p>;
  }

  const filters = {
    type: "messaging",
    members: { $in: ["admin_1"] },
  };

  const sort = {
    last_message_at: -1,
  };

  return (
    <div style={{ height: "calc(100vh - 80px)", display: "flex" }}>
      <Chat client={client}>
        <div style={{ width: 320, borderRight: "1px solid #eee" }}>
          <ChannelList
            filters={filters}
            sort={sort}
            setActiveChannelOnMount
          />
        </div>

        <div style={{ flex: 1 }}>
          <Channel>
            <Window>
              <ChannelHeader />
              <MessageList />
              <MessageComposer />
            </Window>
          </Channel>
        </div>
      </Chat>
    </div>
  );
}

export default AdminChatPage;