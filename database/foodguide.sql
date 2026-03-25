CREATE DATABASE IF NOT EXISTS tourdb;
USE tourdb;

DROP TABLE IF EXISTS Locations;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) CHARACTER SET utf8mb4 NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) CHARACTER SET utf8mb4 NOT NULL,
    Phone VARCHAR(30) NULL,
    Gender VARCHAR(10) NULL,
    Avatar LONGTEXT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'user',
    IsLocked BIT NOT NULL DEFAULT b'0'
);

INSERT INTO Users (Username, Password, FullName, Phone, Gender, Avatar, Role, IsLocked) VALUES
  ('nguyenvana', '123456', 'Nguyen Van A', NULL, NULL, NULL, 'user', b'0'),
  ('tranthib', 'password123', 'Tran Thi B', NULL, NULL, NULL, 'user', b'0'),
  ('traveler99', 'dulichvietnam', 'Du Khach 99', NULL, NULL, NULL, 'user', b'0'),
  ('admin', 'admin123', 'Administrator', '0900000000', 'Nam', NULL, 'admin', b'0');

CREATE TABLE Locations (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) CHARACTER SET utf8mb4,
    Description TEXT CHARACTER SET utf8mb4,
    Image VARCHAR(255),
    Images LONGTEXT CHARACTER SET utf8mb4,
    Audio VARCHAR(255),
    Latitude DOUBLE,
    Longitude DOUBLE,
    TextVi LONGTEXT CHARACTER SET utf8mb4,
    TextEn LONGTEXT CHARACTER SET utf8mb4,
    TextZh LONGTEXT CHARACTER SET utf8mb4,
    TextDe LONGTEXT CHARACTER SET utf8mb4
);

INSERT INTO Locations (Name, Description, Image, Images, Audio, Latitude, Longitude, TextVi, TextEn, TextZh, TextDe)
VALUES
(
  'Ốc Vĩnh Khánh',
  'Phố Vĩnh Khánh nổi tiếng với các món ốc tươi ngon và không khí ẩm thực sôi động về đêm.',
  'https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800',
  '["https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800","https://images.unsplash.com/photo-1559847844-5315695dadae?w=800","https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=800","https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=800"]',
  'oc.mp3',
  10.7593,
  106.7046,
  'Chào mừng bạn đến với quán Ốc Vĩnh Khánh! Đây là một trong những quán ốc lâu đời và nổi tiếng nhất tại Phố Ẩm thực Vĩnh Khánh, Quận 4, Thành phố Hồ Chí Minh. Quán phục vụ đa dạng các món ốc tươi ngon như ốc len xào dừa, ốc hương nướng mỡ hành, ốc bươu hấp sả, và nhiều món hải sản đặc sắc khác. Nguyên liệu được chọn lọc tươi sống mỗi ngày, chế biến theo công thức gia truyền với hương vị đậm đà, cay nồng đặc trưng. Hãy dừng chân và thưởng thức bữa ăn đêm tuyệt vời tại đây!',
  'Welcome to Oc Vinh Khanh! This is one of the oldest and most famous snail restaurants on Vinh Khanh Food Street, District 4, Ho Chi Minh City. The restaurant serves a wide variety of fresh snail dishes such as mud creeper snails stir-fried with coconut milk, grilled snails with spring onion butter, steamed apple snails with lemongrass, and many other special seafood dishes. Ingredients are freshly selected every day and prepared using traditional family recipes with rich, spicy flavors. Stop by and enjoy a wonderful night meal here!',
  '欢迎来到Ốc Vĩnh Khánh！这里是胡志明市第四郡永庆美食街上历史悠久且非常有名的螺蛳海鲜餐厅之一。店里提供多种新鲜美味的螺类料理，例如椰汁炒螺、葱油烤香螺、香茅蒸田螺，以及许多特色海鲜菜肴。每天严选新鲜食材，并按照传统配方烹调，风味浓郁又带有越式香辣特色。欢迎在这里停下脚步，享受一顿精彩的夜宵美食！',
  'Willkommen bei Oc Vinh Khanh! Dies ist eines der altesten und bekanntesten Schneckenrestaurants in der Vinh Khanh Essensstrasse im Bezirk 4, Ho-Chi-Minh-Stadt. Das Restaurant serviert eine grosse Auswahl an frischen Schneckengerichten wie in Kokosmilch gebratene Schlammschnecken, gegrillte Schnecken mit Fruhlingszwiebelol, gedampfte Apfelschnecken mit Zitronengras und viele andere besondere Meeresfruchtegerichte. Die Zutaten werden taglich frisch ausgewahlt und nach traditionellen Familienrezepten mit reichhaltigen, wurzigen Aromen zubereitet. Halten Sie an und geniessen Sie hier ein wunderbares Abendessen!'
),
(
  'Phá Lấu Vĩnh Khánh',
  'Phá lấu là món ăn đường phố nổi tiếng với hương vị đậm đà, béo thơm và rất đặc trưng của Sài Gòn.',
  'https://images.unsplash.com/photo-1555126634-323283e090fa?w=800',
  '["https://images.unsplash.com/photo-1555126634-323283e090fa?w=800","https://images.unsplash.com/photo-1547592180-85f173990554?w=800","https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=800","https://images.unsplash.com/photo-1512058564366-18510be2db19?w=800"]',
  'phalau.mp3',
  10.7596,
  106.7049,
  'Chào mừng bạn đến với quán Phá Lấu Vĩnh Khánh! Phá lấu là món ăn đường phố đặc trưng của người Hoa tại Sài Gòn, được chế biến từ các bộ phận nội tạng như lòng, tim, gan, phổi heo hoặc bò, hầm nhuyễn trong nước dừa cùng các loại gia vị ngũ vị hương thơm nức. Quán Phá Lấu Vĩnh Khánh nổi tiếng với nước hầm đậm đà, thịt mềm tan, ăn kèm bánh mì giòn rụm hoặc hủ tiếu dai ngon. Đây là địa chỉ quen thuộc của người dân địa phương và du khách khi ghé thăm phố ẩm thực Vĩnh Khánh về đêm.',
  'Welcome to Pha Lau Vinh Khanh! Pha Lau is a signature street food of the Chinese community in Saigon, made from offal such as intestines, heart, liver, and lungs of pork or beef, slow-braised in coconut water with five-spice seasoning. Pha Lau Vinh Khanh is famous for its rich braising broth, tender melt-in-your-mouth meat, served with crispy baguette or chewy rice noodles. This is a familiar address for locals and visitors when exploring Vinh Khanh food street at night.',
  '欢迎来到Phá Lấu Vĩnh Khánh！法式卤杂是西贡华人社区极具代表性的街头美食，通常以猪或牛的内脏，如肠、心、肝、肺等，配合椰汁和五香慢火炖煮而成。Phá Lấu Vĩnh Khánh以汤底浓郁、口感软嫩闻名，可搭配香脆法棍或有嚼劲的河粉一起享用。来到永庆美食街夜游时，这里是本地人和游客都很熟悉的热门去处。',
  'Willkommen bei Pha Lau Vinh Khanh! Pha Lau ist ein charakteristisches Strassenessen der chinesischen Gemeinschaft in Saigon, das aus Innereien wie Darm, Herz, Leber und Lunge von Schwein oder Rind hergestellt wird, die in Kokoswasser mit Funf-Gewurze-Wurzung langsam geschmort werden. Pha Lau Vinh Khanh ist bekannt fur seine reichhaltige Schmorsauce, zartes Fleisch, das auf der Zunge zergeht, serviert mit knusprigem Baguette oder zahen Reisnudeln. Dies ist eine vertraute Adresse fur Einheimische und Besucher beim Erkunden der Vinh Khanh Essensstrasse bei Nacht.'
),
(
  'Chè Vĩnh Khánh',
  'Khu phố này có nhiều quán chè ngon, đa dạng hương vị và rất được yêu thích vào buổi tối.',
  'https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800',
  '["https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800","https://images.unsplash.com/photo-1488477181946-6428a0291777?w=800","https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=800","https://images.unsplash.com/photo-1497534446932-c925b458314e?w=800"]',
  'che.mp3',
  10.7599,
  106.7052,
  'Chào mừng bạn đến với quán Chè Vĩnh Khánh! Đây là thiên đường chè ngọt giữa lòng phố ẩm thực Vĩnh Khánh. Quán phục vụ hơn 30 loại chè khác nhau như chè Thái, chè khúc bạch, chè bưởi, chè đậu xanh đánh, chè trôi nước, và nhiều loại chè truyền thống Nam Bộ đặc sắc. Mỗi bát chè được chế biến tỉ mỉ từ nguyên liệu tươi ngon, ngọt thanh vừa phải, mát lạnh giải nhiệt hoàn hảo cho những buổi tối Sài Gòn oi bức. Ghé thăm và thử ngay những bát chè thơm ngon tại đây!',
  'Welcome to Che Vinh Khanh! This is a sweet dessert paradise in the heart of Vinh Khanh food street. The shop serves over 30 different types of Vietnamese sweet soups including Thai-style che, che khuc bach, pomelo che, mung bean che, glutinous rice ball che, and many other traditional Southern Vietnamese desserts. Each bowl is carefully prepared from fresh ingredients, perfectly sweetened, and served cold - the perfect refreshment for hot Saigon evenings. Come visit and try the delicious sweet soups here!',
  '欢迎来到Chè Vĩnh Khánh！这里是永庆美食街中心的一处甜品天堂。店内提供超过三十种越南传统甜汤与特色甜品，例如泰式甜汤、杏仁奶冻甜汤、柚子甜汤、绿豆甜汤、汤圆甜汤等。每一碗甜品都以新鲜食材细心制作，甜度适中、冰凉清爽，非常适合在闷热的西贡夜晚享用。欢迎来这里品尝香甜可口的越式甜品！',
  'Willkommen bei Che Vinh Khanh! Dies ist ein susses Dessert-Paradies im Herzen der Vinh Khanh Essensstrasse. Der Laden serviert uber 30 verschiedene Arten vietnamesischer susser Suppen, darunter Thai-Stil Che, Che Khuc Bach, Pampelmuse-Che, Mungbohnen-Che, Klebreisball-Che und viele andere traditionelle sudvietnamesische Desserts. Jede Schussel wird sorgfaltig aus frischen Zutaten zubereitet, perfekt gesusst und kalt serviert - die perfekte Erfrischung fur heisse Saigon-Abende. Kommen Sie und probieren Sie die kostlichen sussen Suppen hier!'
),
(
  'Hải sản nướng',
  'Hải sản nướng tại đây nổi bật với nguyên liệu tươi sống, mùi thơm hấp dẫn và cách chế biến đậm vị.',
  'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800',
  '["https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800","https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800","https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800","https://images.unsplash.com/photo-1482049016688-2d3e1b311543?w=800"]',
  'seafood.mp3',
  10.7601,
  106.7053,
  'Chào mừng bạn đến với quán Hải Sản Nướng Vĩnh Khánh! Đây là điểm đến lý tưởng cho những tín đồ hải sản tươi sống tại phố ẩm thực Vĩnh Khánh. Quán nổi tiếng với các món nướng than hoa thơm lừng như tôm hùm nướng bơ tỏi, mực nướng sa tế, sò điệp nướng phô mai, cua rang muối, và ghẹ hấp bia. Hải sản được nhập trực tiếp từ các cảng cá mỗi sáng, đảm bảo độ tươi ngon tuyệt đối. Không khí vỉa hè sôi động, khói nướng thơm ngào ngạt tạo nên trải nghiệm ẩm thực đường phố đặc sắc không thể bỏ qua khi đến Sài Gòn.',
  'Welcome to Vinh Khanh Grilled Seafood! This is the ideal destination for fresh seafood lovers on Vinh Khanh food street. The restaurant is famous for its fragrant charcoal-grilled dishes such as garlic butter grilled lobster, satay grilled squid, cheese grilled scallops, salt and pepper crab, and beer-steamed blue crab. Seafood is imported directly from fishing ports every morning, ensuring absolute freshness. The lively sidewalk atmosphere and aromatic grilling smoke create an unforgettable street food experience not to be missed when visiting Saigon.',
  '欢迎来到Vĩnh Khánh海鲜烧烤！这里是永庆美食街上热爱新鲜海鲜食客的理想目的地。餐厅以炭火烧烤海鲜闻名，例如蒜香黄油烤龙虾、沙爹烤鱿鱼、芝士烤扇贝、椒盐蟹和啤酒蒸花蟹等。所有海鲜每天清晨直接从渔港采购，确保绝对新鲜。热闹的街边氛围与扑鼻而来的烧烤香气，共同构成到访西贡时不容错过的街头美食体验。',
  'Willkommen bei Vinh Khanh Gegrillten Meeresfruchten! Dies ist das ideale Ziel fur Liebhaber frischer Meeresfruchte in der Vinh Khanh Essensstrasse. Das Restaurant ist bekannt fur seine duftenden Holzkohle-Grillgerichte wie mit Knoblauchbutter gegrillten Hummer, Satay-gegrillten Tintenfisch, mit Kase uberbackene Jakobsmuscheln, Salz-und-Pfeffer-Krabbe und biergekochte Schwimmkrabbe. Meeresfruchte werden jeden Morgen direkt von Fischereihafen importiert, um absolute Frische zu gewahrleisten. Die lebhafte Burgersteig-Atmosphare und der aromatische Grillrauch schaffen ein unvergessliches Strassenessen-Erlebnis, das beim Besuch Saigons nicht verpasst werden sollte.'
);

SELECT * FROM Users;
SELECT * FROM Locations;
