CREATE DATABASE IF NOT EXISTS tourdb;
USE tourdb;

CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) CHARACTER SET utf8mb4 UNIQUE,
    Password VARCHAR(255)
);

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
  'Phố Vĩnh Khánh nổi tiếng với các món ốc...',
  'https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800',
  '["https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800","https://images.unsplash.com/photo-1559847844-5315695dadae?w=800","https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=800","https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=800"]', 'oc.mp3', 10.7593, 106.7046,
  'Chào mừng bạn đến với quán Ốc Vĩnh Khánh! Đây là một trong những quán ốc lâu đời và nổi tiếng nhất tại Phố Ẩm Thực Vĩnh Khánh, Quận 4, Thành phố Hồ Chí Minh. Quán phục vụ đa dạng các món ốc tươi ngon như ốc len xào dừa, ốc hương nướng mỡ hành, ốc bươu hấp sả, và nhiều món hải sản đặc sắc khác. Nguyên liệu được chọn lọc tươi sống mỗi ngày, chế biến theo công thức gia truyền với hương vị đậm đà, cay nồng đặc trưng. Hãy dừng chân và thưởng thức bữa ăn đêm tuyệt vời tại đây!',
  'Welcome to Oc Vinh Khanh! This is one of the oldest and most famous snail restaurants on Vinh Khanh Food Street, District 4, Ho Chi Minh City. The restaurant serves a wide variety of fresh snail dishes such as mud creeper snails stir-fried with coconut milk, grilled snails with spring onion butter, steamed apple snails with lemongrass, and many other special seafood dishes. Ingredients are freshly selected every day and prepared using traditional family recipes with rich, spicy flavors. Stop by and enjoy a wonderful night meal here!',
  '欢迎来到永庆螺蛳餐厅！这是胡志明市第四郡永庆美食街上历史最悠久、最著名的螺蛳餐厅之一。餐厅供应各种新鲜螺蛳菜肴，如椰汁炒泥螺、葱油烤香螺、香茅蒸田螺以及其他特色海鲜菜肴。食材每天新鲜挑选，按照传统家传秘方烹制，口味浓郁香辣。请停下来，在这里享用一顿美妙的夜宵吧！',
  'Willkommen bei Oc Vinh Khanh! Dies ist eines der ältesten und bekanntesten Schneckenrestaurants in der Vinh Khanh Essensstraße im Bezirk 4, Ho-Chi-Minh-Stadt. Das Restaurant serviert eine große Auswahl an frischen Schneckengerichten wie in Kokosmilch gebratene Schlammschnecken, gegrillte Schnecken mit Frühlingszwiebelöl, gedämpfte Apfelschnecken mit Zitronengras und viele andere besondere Meeresfrüchtegerichte. Die Zutaten werden täglich frisch ausgewählt und nach traditionellen Familienrezepten mit reichhaltigen, würzigen Aromen zubereitet. Halten Sie an und genießen Sie hier ein wunderbares Abendessen!'
),
(
  'Phá lấu Vĩnh Khánh',
  'Phá lấu là món ăn đường phố nổi tiếng...',
  'https://images.unsplash.com/photo-1555126634-323283e090fa?w=800',
  '["https://images.unsplash.com/photo-1555126634-323283e090fa?w=800","https://images.unsplash.com/photo-1547592180-85f173990554?w=800","https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=800","https://images.unsplash.com/photo-1512058564366-18510be2db19?w=800"]', 'phalau.mp3', 10.7596, 106.7049,
  'Chào mừng bạn đến với quán Phá Lấu Vĩnh Khánh! Phá lấu là món ăn đường phố đặc trưng của người Hoa tại Sài Gòn, được chế biến từ các bộ phận nội tạng như lòng, tim, gan, phổi heo hoặc bò, hầm nhừ trong nước dừa cùng các loại gia vị ngũ vị hương thơm nức. Quán Phá Lấu Vĩnh Khánh nổi tiếng với nước hầm đậm đà, thịt mềm tan, ăn kèm bánh mì giòn rụm hoặc hủ tiếu dai ngon. Đây là địa chỉ quen thuộc của người dân địa phương và du khách khi ghé thăm phố ẩm thực Vĩnh Khánh về đêm.',
  'Welcome to Pha Lau Vinh Khanh! Pha Lau is a signature street food of the Chinese community in Saigon, made from offal such as intestines, heart, liver, and lungs of pork or beef, slow-braised in coconut water with five-spice seasoning. Pha Lau Vinh Khanh is famous for its rich braising broth, tender melt-in-your-mouth meat, served with crispy baguette or chewy rice noodles. This is a familiar address for locals and visitors when exploring Vinh Khanh food street at night.',
  '欢迎来到永庆卤味摊！卤味是西贡华人社区的标志性街头小吃，由猪或牛的内脏如肠、心、肝、肺制成，用椰子水和五香料慢炖而成。永庆卤味以其浓郁的卤汁、软烂入口即化的肉质而闻名，搭配酥脆法棍或劲道米粉食用。这是当地居民和游客夜游永庆美食街时必去的熟悉地址。',
  'Willkommen bei Pha Lau Vinh Khanh! Pha Lau ist ein charakteristisches Straßenessen der chinesischen Gemeinschaft in Saigon, das aus Innereien wie Darm, Herz, Leber und Lunge von Schwein oder Rind hergestellt wird, die in Kokoswasser mit Fünf-Gewürze-Würzung langsam geschmort werden. Pha Lau Vinh Khanh ist bekannt für seine reichhaltige Schmorsauce, zartes Fleisch, das auf der Zunge zergeht, serviert mit knusprigem Baguette oder zähen Reisnudeln. Dies ist eine vertraute Adresse für Einheimische und Besucher beim Erkunden der Vinh Khanh Essensstraße bei Nacht.'
),
(
  'Chè Vĩnh Khánh',
  'Khu phố có nhiều quán chè...',
  'https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800',
  '["https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800","https://images.unsplash.com/photo-1488477181946-6428a0291777?w=800","https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=800","https://images.unsplash.com/photo-1497534446932-c925b458314e?w=800"]', 'che.mp3', 10.7599, 106.7052,
  'Chào mừng bạn đến với quán Chè Vĩnh Khánh! Đây là thiên đường chè ngọt giữa lòng phố ẩm thực Vĩnh Khánh. Quán phục vụ hơn 30 loại chè khác nhau như chè thái, chè khúc bạch, chè bưởi, chè đậu xanh đánh, chè trôi nước, và nhiều loại chè truyền thống Nam Bộ đặc sắc. Mỗi bát chè được chế biến tỉ mỉ từ nguyên liệu tươi ngon, ngọt thanh vừa phải, mát lạnh giải nhiệt hoàn hảo cho những buổi tối Sài Gòn oi bức. Ghé thăm và thử ngay những bát chè thơm ngon tại đây!',
  'Welcome to Che Vinh Khanh! This is a sweet dessert paradise in the heart of Vinh Khanh food street. The shop serves over 30 different types of Vietnamese sweet soups including Thai-style che, che khuc bach, pomelo che, mung bean che, glutinous rice ball che, and many other traditional Southern Vietnamese desserts. Each bowl is carefully prepared from fresh ingredients, perfectly sweetened, and served cold — the perfect refreshment for hot Saigon evenings. Come visit and try the delicious sweet soups here!',
  '欢迎来到永庆甜品店！这是永庆美食街中心的甜品天堂。店铺供应30多种不同的越南甜汤，包括泰式甜汤、白玉甜汤、柚子甜汤、绿豆甜汤、汤圆甜汤以及许多其他传统南越甜品。每碗甜品都用新鲜食材精心制作，甜度适中，冰凉爽口，是炎热西贡夜晚的完美消暑饮品。快来品尝这里美味的甜汤吧！',
  'Willkommen bei Che Vinh Khanh! Dies ist ein süßes Dessert-Paradies im Herzen der Vinh Khanh Essensstraße. Der Laden serviert über 30 verschiedene Arten vietnamesischer süßer Suppen, darunter Thai-Stil Che, Che Khuc Bach, Pampelmuse-Che, Mungbohnen-Che, Klebreisball-Che und viele andere traditionelle südvietnamesische Desserts. Jede Schüssel wird sorgfältig aus frischen Zutaten zubereitet, perfekt gesüßt und kalt serviert — die perfekte Erfrischung für heiße Saigon-Abende. Kommen Sie und probieren Sie die köstlichen süßen Suppen hier!'
),
(
  'Hải sản nướng',
  'Hải sản nướng...',
  'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800',
  '["https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800","https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800","https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800","https://images.unsplash.com/photo-1482049016688-2d3e1b311543?w=800"]', 'seafood.mp3', 10.7601, 106.7053,
  'Chào mừng bạn đến với quán Hải Sản Nướng Vĩnh Khánh! Đây là điểm đến lý tưởng cho những tín đồ hải sản tươi sống tại phố ẩm thực Vĩnh Khánh. Quán nổi tiếng với các món nướng than hoa thơm lừng như tôm hùm nướng bơ tỏi, mực nướng sa tế, sò điệp nướng phô mai, cua rang muối, và ghẹ hấp bia. Hải sản được nhập trực tiếp từ các cảng cá mỗi sáng, đảm bảo độ tươi ngon tuyệt đối. Không khí vỉa hè sôi động, khói nướng thơm ngào ngạt tạo nên trải nghiệm ẩm thực đường phố đặc sắc không thể bỏ qua khi đến Sài Gòn.',
  'Welcome to Vinh Khanh Grilled Seafood! This is the ideal destination for fresh seafood lovers on Vinh Khanh food street. The restaurant is famous for its fragrant charcoal-grilled dishes such as garlic butter grilled lobster, satay grilled squid, cheese grilled scallops, salt and pepper crab, and beer-steamed blue crab. Seafood is imported directly from fishing ports every morning, ensuring absolute freshness. The lively sidewalk atmosphere and aromatic grilling smoke create an unforgettable street food experience not to be missed when visiting Saigon.',
  '欢迎来到永庆烤海鲜！这是永庆美食街新鲜海鲜爱好者的理想目的地。餐厅以香气四溢的炭烤菜肴而闻名，如蒜香黄油烤龙虾、沙爹烤鱿鱼、芝士烤扇贝、椒盐螃蟹和啤酒蒸梭子蟹。海鲜每天早晨直接从渔港进货，确保绝对新鲜。热闹的街边氛围和诱人的烧烤香气创造了一种难忘的街头美食体验，是游览西贡时不可错过的地方。',
  'Willkommen bei Vinh Khanh Gegrillten Meeresfrüchten! Dies ist das ideale Ziel für Liebhaber frischer Meeresfrüchte in der Vinh Khanh Essensstraße. Das Restaurant ist bekannt für seine duftenden Holzkohle-Grillgerichte wie mit Knoblauchbutter gegrillten Hummer, Satay-gegrillten Tintenfisch, mit Käse überbackene Jakobsmuscheln, Salz-und-Pfeffer-Krabbe und biergekochte Schwimmkrabbe. Meeresfrüchte werden jeden Morgen direkt von Fischereihäfen importiert, um absolute Frische zu gewährleisten. Die lebhafte Bürgersteig-Atmosphäre und der aromatische Grillrauch schaffen ein unvergessliches Straßenessen-Erlebnis, das beim Besuch Saigons nicht verpasst werden sollte.'
);

INSERT INTO Users (Username, Password) VALUES ('admin', 'admin123');

SELECT * FROM Locations;
