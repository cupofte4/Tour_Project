CREATE DATABASE IF NOT EXISTS tourdb;
USE tourdb;

DROP TABLE IF EXISTS LocationManagerAssignments;
DROP TABLE IF EXISTS LocationStats;
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
  ('nguyenvana', '123456', 'Nguyễn Văn A', NULL, NULL, NULL, 'user', b'0'),
  ('tranthib', 'password123', 'Trần Thị B', NULL, NULL, NULL, 'user', b'0'),
  ('traveler99', 'dulichvietnam', 'Du Khách 99', NULL, NULL, NULL, 'user', b'0'),
  ('manager1', 'manager123', 'Business Manager', '0911000000', NULL, NULL, 'manager', b'0'),
  ('admin', 'admin123', 'Administrator', '0900000000', 'Nam', NULL, 'admin', b'0');

CREATE TABLE Locations (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) CHARACTER SET utf8mb4 NOT NULL,
    Description TEXT CHARACTER SET utf8mb4 NOT NULL,
    Image VARCHAR(255) NOT NULL,
    Images LONGTEXT CHARACTER SET utf8mb4 NULL,
    Address VARCHAR(255) CHARACTER SET utf8mb4 NOT NULL DEFAULT '',
    Phone VARCHAR(30) NOT NULL DEFAULT '',
    ReviewsJson LONGTEXT CHARACTER SET utf8mb4 NOT NULL,
    Latitude DOUBLE NOT NULL,
    Longitude DOUBLE NOT NULL,
    TextVi LONGTEXT CHARACTER SET utf8mb4 NULL,
    TextEn LONGTEXT CHARACTER SET utf8mb4 NULL,
    TextZh LONGTEXT CHARACTER SET utf8mb4 NULL,
    TextDe LONGTEXT CHARACTER SET utf8mb4 NULL
);

CREATE TABLE LocationStats (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    LocationId INT NOT NULL,
    StatDate DATE NOT NULL,
    ViewsCount INT NOT NULL DEFAULT 0,
    AudioPlaysCount INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_LocationStats_Locations FOREIGN KEY (LocationId) REFERENCES Locations(Id) ON DELETE CASCADE,
    UNIQUE KEY UX_LocationStats_Location_Date (LocationId, StatDate),
    KEY IX_LocationStats_StatDate (StatDate)
);

CREATE TABLE LocationManagerAssignments (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ManagerId INT NOT NULL,
    LocationId INT NOT NULL,
    CONSTRAINT FK_LocationManagerAssignments_Users FOREIGN KEY (ManagerId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LocationManagerAssignments_Locations FOREIGN KEY (LocationId) REFERENCES Locations(Id) ON DELETE CASCADE,
    UNIQUE KEY UX_LocationManagerAssignments_Manager_Location (ManagerId, LocationId)
);

INSERT INTO Locations (
    Name, Description, Image, Images, Address, Phone, ReviewsJson,
    Latitude, Longitude, TextVi, TextEn, TextZh, TextDe
)
VALUES
(
  'Ốc Vĩnh Khánh',
  'Phố Vĩnh Khánh nổi tiếng với các món ốc tươi ngon và không khí ẩm thực sôi động về đêm.',
  'https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800',
  '["https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800","https://images.unsplash.com/photo-1559847844-5315695dadae?w=800","https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=800","https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=800"]',
  '16 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0909123456',
  '[{"author":"Minh Anh","rating":5,"comment":"Ốc tươi, nước chấm rất cuốn và quán đông vui về đêm.","createdAt":"2026-03-20T19:30:00Z"},{"author":"Daniel","rating":4,"comment":"Great variety of snails and fast service, a bit crowded at peak time.","createdAt":"2026-03-18T12:15:00Z"}]',
  10.7593,
  106.7046,
  'Chào mừng bạn đến với quán Ốc Vĩnh Khánh! Đây là một trong những quán ốc lâu đời và nổi tiếng nhất tại phố ẩm thực Vĩnh Khánh, Quận 4, Thành phố Hồ Chí Minh. Quán phục vụ đa dạng các món ốc tươi ngon như ốc len xào dừa, ốc hương nướng mỡ hành, ốc bươu hấp sả và nhiều món hải sản đặc sắc khác. Nguyên liệu được chọn lọc tươi sống mỗi ngày, chế biến theo công thức gia truyền với hương vị đậm đà, cay nồng đặc trưng. Hãy dừng chân và thưởng thức bữa ăn đêm tuyệt vời tại đây!',
  'Welcome to Oc Vinh Khanh! This is one of the oldest and most famous snail restaurants on Vinh Khanh Food Street, District 4, Ho Chi Minh City. The restaurant serves a wide variety of fresh snail dishes such as mud creeper snails stir-fried with coconut milk, grilled snails with spring onion butter, steamed apple snails with lemongrass, and many other special seafood dishes. Ingredients are freshly selected every day and prepared using traditional family recipes with rich, spicy flavors. Stop by and enjoy a wonderful night meal here!',
  '欢迎来到永庆螺店。这里是胡志明市第四郡永庆美食街上历史悠久且非常有名的海鲜小店之一。店里供应多种新鲜螺类和海鲜，例如椰汁炒螺、葱油烤螺、香茅蒸螺等，味道浓郁，很适合夜晚来品尝地道的西贡街头美食。',
  'Willkommen bei Oc Vinh Khanh! Dies ist eines der bekanntesten Schneckenrestaurants in der Vinh Khanh Essensstrasse im Bezirk 4.'
),
(
  'Phá Lấu Vĩnh Khánh',
  'Phá lấu là món ăn đường phố nổi tiếng với hương vị đậm đà, béo thơm và rất đặc trưng của Sài Gòn.',
  'https://images.unsplash.com/photo-1555126634-323283e090fa?w=800',
  '["https://images.unsplash.com/photo-1555126634-323283e090fa?w=800","https://images.unsplash.com/photo-1547592180-85f173990554?w=800","https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=800","https://images.unsplash.com/photo-1512058564366-18510be2db19?w=800"]',
  '22 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0918234567',
  '[{"author":"Thanh","rating":5,"comment":"Phá lấu béo thơm, bánh mì giòn và nước chấm rất hợp.","createdAt":"2026-03-22T18:05:00Z"},{"author":"Sophie","rating":4,"comment":"Flavorful broth and tender meat, nice stop for late-night food.","createdAt":"2026-03-19T13:40:00Z"}]',
  10.7596,
  106.7049,
  'Chào mừng bạn đến với quán Phá Lấu Vĩnh Khánh! Phá lấu là món ăn đường phố đặc trưng của người Hoa tại Sài Gòn, được chế biến từ các bộ phận nội tạng như lòng, tim, gan, phổi heo hoặc bò, hầm nhừ trong nước dừa cùng các loại gia vị ngũ vị hương thơm nức. Quán Phá Lấu Vĩnh Khánh nổi tiếng với nước hầm đậm đà, thịt mềm tan, ăn kèm bánh mì giòn rụm hoặc hủ tiếu dai ngon. Đây là địa chỉ quen thuộc của người dân địa phương và du khách khi ghé thăm phố ẩm thực Vĩnh Khánh về đêm.',
  'Welcome to Pha Lau Vinh Khanh! Pha Lau is a signature street food of the Chinese community in Saigon, made from offal such as intestines, heart, liver, and lungs of pork or beef, slow-braised in coconut water with five-spice seasoning. Pha Lau Vinh Khanh is famous for its rich braising broth, tender melt-in-your-mouth meat, served with crispy baguette or chewy rice noodles. This is a familiar address for locals and visitors when exploring Vinh Khanh food street at night.',
  '欢迎来到永庆破烂粉小店。破烂粉是西贡很有代表性的街头美食，通常用猪或牛的内脏慢炖而成，汤汁浓郁，香料丰富。这里的口味醇厚，搭配法棍或面条都很受欢迎，是第四郡夜间觅食时不可错过的一站。',
  'Willkommen bei Pha Lau Vinh Khanh! Hier gibt es reichhaltiges geschmortes Innereiengericht im typischen Saigon Stil.'
),
(
  'Chè Vĩnh Khánh',
  'Khu phố này có nhiều quán chè ngon, đa dạng hương vị và rất được yêu thích vào buổi tối.',
  'https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800',
  '["https://images.unsplash.com/photo-1541696432-82c6da8ce7bf?w=800","https://images.unsplash.com/photo-1488477181946-6428a0291777?w=800","https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=800","https://images.unsplash.com/photo-1497534446932-c925b458314e?w=800"]',
  '35 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0937345678',
  '[{"author":"Linh","rating":5,"comment":"Chè mát, ngọt vừa phải và rất nhiều lựa chọn.","createdAt":"2026-03-21T14:20:00Z"},{"author":"Kevin","rating":4,"comment":"Refreshing desserts after seafood, especially the pomelo sweet soup.","createdAt":"2026-03-17T20:10:00Z"}]',
  10.7599,
  106.7052,
  'Chào mừng bạn đến với quán Chè Vĩnh Khánh! Đây là thiên đường chè ngọt giữa lòng phố ẩm thực Vĩnh Khánh. Quán phục vụ hơn 30 loại chè khác nhau như chè Thái, chè khúc bạch, chè bưởi, chè đậu xanh đánh, chè trôi nước và nhiều loại chè truyền thống Nam Bộ đặc sắc. Mỗi bát chè được chế biến tỉ mỉ từ nguyên liệu tươi ngon, ngọt thanh vừa phải, mát lạnh giải nhiệt hoàn hảo cho những buổi tối Sài Gòn oi bức. Ghé thăm và thử ngay những bát chè thơm ngon tại đây!',
  'Welcome to Che Vinh Khanh! This is a sweet dessert paradise in the heart of Vinh Khanh food street. The shop serves over 30 different types of Vietnamese sweet soups including Thai-style che, che khuc bach, pomelo che, mung bean che, glutinous rice ball che, and many other traditional Southern Vietnamese desserts. Each bowl is carefully prepared from fresh ingredients, perfectly sweetened, and served cold - the perfect refreshment for hot Saigon evenings. Come visit and try the delicious sweet soups here!',
  '欢迎来到永庆甜品店。这里有多种越南传统甜汤和冰甜品，例如泰式甜汤、柚子甜汤、绿豆甜汤和水晶甜品等。甜度适中，口感清爽，非常适合在炎热的西贡夜晚作为饭后甜点。',
  'Willkommen bei Che Vinh Khanh! Dies ist ein beliebter Dessertladen mit vielen vietnamesischen Sussspeisen.'
),
(
  'Hải Sản Nướng',
  'Hải sản nướng tại đây nổi bật với nguyên liệu tươi sống, mùi thơm hấp dẫn và cách chế biến đậm vị.',
  'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800',
  '["https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800","https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800","https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800","https://images.unsplash.com/photo-1482049016688-2d3e1b311543?w=800"]',
  '48 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0988456789',
  '[{"author":"Quốc Huy","rating":5,"comment":"Hải sản tươi, nướng lên thơm và không khí vỉa hè rất đã.","createdAt":"2026-03-23T17:55:00Z"},{"author":"Maria","rating":4,"comment":"Fresh scallops and crab, lively street-side atmosphere.","createdAt":"2026-03-16T11:25:00Z"}]',
  10.7601,
  106.7053,
  'Chào mừng bạn đến với quán Hải Sản Nướng Vĩnh Khánh! Đây là điểm đến lý tưởng cho những tín đồ hải sản tươi sống tại phố ẩm thực Vĩnh Khánh. Quán nổi tiếng với các món nướng than hoa thơm lừng như tôm hùm nướng bơ tỏi, mực nướng sa tế, sò điệp nướng phô mai, cua rang muối và ghẹ hấp bia. Hải sản được nhập trực tiếp từ các cảng cá mỗi sáng, đảm bảo độ tươi ngon tuyệt đối. Không khí vỉa hè sôi động, khói nướng thơm ngào ngạt tạo nên trải nghiệm ẩm thực đường phố đặc sắc không thể bỏ qua khi đến Sài Gòn.',
  'Welcome to Vinh Khanh Grilled Seafood! This is the ideal destination for fresh seafood lovers on Vinh Khanh food street. The restaurant is famous for its fragrant charcoal-grilled dishes such as garlic butter grilled lobster, satay grilled squid, cheese grilled scallops, salt and pepper crab, and beer-steamed blue crab. Seafood is imported directly from fishing ports every morning, ensuring absolute freshness. The lively sidewalk atmosphere and aromatic grilling smoke create an unforgettable street food experience not to be missed when visiting Saigon.',
  '欢迎来到永庆炭烤海鲜店。这里以新鲜海鲜和炭火现烤闻名，有蒜蓉黄油龙虾、沙爹烤鱿鱼、芝士烤扇贝、椒盐螃蟹等招牌菜。摊位气氛热闹，香气四溢，是体验西贡夜市海鲜文化的热门地点。',
  'Willkommen bei Vinh Khanh Gegrillten Meeresfruchten! Hier gibt es frische Meeresfruchte und lebhafte Strassenatmosphare.'
),
(
  'Bánh Tráng Nướng Q4',
  'Quán bánh tráng nướng giòn thơm với nhiều loại topping, thích hợp ăn vặt buổi tối.',
  'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800',
  '["https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800","https://images.unsplash.com/photo-1544025162-d76694265947?w=800","https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800"]',
  '52 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0903456789',
  '[{"author":"Như","rating":5,"comment":"Bánh giòn, topping đầy đặn và sốt rất cuốn.","createdAt":"2026-03-24T18:45:00Z"},{"author":"Ryan","rating":4,"comment":"Crispy and flavorful, great for a quick snack.","createdAt":"2026-03-19T10:20:00Z"}]',
  10.7603,
  106.7055,
  'Chào mừng bạn đến với quán Bánh Tráng Nướng Quận 4. Đây là điểm dừng chân quen thuộc của nhiều bạn trẻ khi dạo quanh phố ẩm thực Vĩnh Khánh. Bánh được nướng trực tiếp trên bếp than, lớp đế giòn thơm, phủ trứng, xúc xích, khô gà, phô mai và nhiều loại sốt đậm vị. Món ăn đơn giản nhưng hấp dẫn nhờ mùi thơm lan tỏa và cảm giác giòn vui miệng khi thưởng thức nóng. Nếu bạn muốn tìm một món ăn vặt nhanh, ngon và đậm chất đường phố Sài Gòn, đây là lựa chọn rất đáng thử.',
  'Welcome to District 4 Grilled Rice Paper. This is a popular stop for young diners exploring Vinh Khanh food street. Each rice paper sheet is grilled over charcoal until crisp, then topped with egg, sausage, shredded chicken floss, cheese, and savory sauces. The dish is simple yet addictive thanks to its smoky aroma and crunchy texture. If you are looking for a quick and tasty Saigon street snack, this is a very good choice.',
  '欢迎来到第四郡烤米纸小店。这里是年轻游客在永庆美食街常去的小吃点。米纸在炭火上现烤，加入鸡蛋、香肠、鸡肉松、奶酪和酱料，口感香脆又有层次。如果你想体验西贡风味的小吃，这里非常值得一试。',
  'Willkommen bei District 4 gegrilltem Reispapier. Dieser Stand ist ein beliebter Snackpunkt auf der Vinh Khanh Strasse. Das Reispapier wird direkt über Holzkohle gegrillt und mit Ei, Wurst, Huhnerflocken, Kase und Wurzsossen belegt. Es ist knusprig, aromatisch und ideal fur einen schnellen Streetfood Genuss in Saigon.'
),
(
  'Súp Cua Vĩnh Khánh',
  'Súp cua nóng hổi với thịt cua, nấm và trứng đánh, phù hợp cho bữa ăn nhẹ về đêm.',
  'https://images.unsplash.com/photo-1547592180-85f173990554?w=800',
  '["https://images.unsplash.com/photo-1547592180-85f173990554?w=800","https://images.unsplash.com/photo-1512058564366-18510be2db19?w=800","https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=800"]',
  '58 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0914567890',
  '[{"author":"Trung","rating":5,"comment":"Súp sánh mịn, nhiều cua và rất ấm bụng.","createdAt":"2026-03-25T19:15:00Z"},{"author":"Emily","rating":4,"comment":"Comforting crab soup with a nice silky texture.","createdAt":"2026-03-21T09:35:00Z"}]',
  10.7604,
  106.7057,
  'Chào mừng bạn đến với quán Súp Cua Vĩnh Khánh. Món súp cua ở đây nổi bật với phần nước súp sánh nhẹ, ngọt thanh từ nước dùng và thịt cua thật. Bên trong bát súp còn có trứng đánh, nấm, bắp non và một chút tiêu tạo cảm giác ấm áp, dễ ăn. Đây là món rất thích hợp cho buổi tối hoặc khi bạn muốn thưởng thức một món nhẹ nhưng vẫn đủ vị. Quán được nhiều thực khách yêu thích nhờ phần topping đầy đặn và hương vị ổn định.',
  'Welcome to Vinh Khanh Crab Soup. The crab soup here is known for its silky broth, gentle sweetness, and generous amount of real crab meat. Each bowl also includes beaten egg, mushrooms, baby corn, and a touch of pepper for warmth and depth. It is a comforting option for the evening or whenever you want something light yet satisfying. Many visitors enjoy this place for its generous toppings and consistently pleasant flavor.',
  '欢迎来到永庆蟹汤店。这里的蟹汤口感顺滑，味道清甜，加入了真实蟹肉、蛋花、蘑菇和玉米笋，再撒上一点胡椒，喝起来非常暖胃。若你想在晚上吃点清爽又有营养的食物，这里是很合适的选择。',
  'Willkommen bei Vinh Khanh Krabbensuppe. Die Suppe ist fur ihre samtige Konsistenz, ihre milde Suesse und das reichliche echte Krabbenfleisch bekannt. Dazu kommen Ei, Pilze, junge Maiskolben und etwas Pfeffer. Sie ist ideal fur einen leichten, aber wohltuenden Abendimbiss.'
),
(
  'Trà Sữa Vỉa Hè',
  'Quầy trà sữa và trà trái cây mát lạnh, rất phù hợp để nghỉ chân sau khi ăn tối.',
  'https://images.unsplash.com/photo-1558857563-b371033873b8?w=800',
  '["https://images.unsplash.com/photo-1558857563-b371033873b8?w=800","https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=800","https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=800"]',
  '64 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0935678901',
  '[{"author":"Vy","rating":4,"comment":"Trà trái cây thơm và topping khá nhiều.","createdAt":"2026-03-23T13:05:00Z"},{"author":"Leo","rating":4,"comment":"Good milk tea and a relaxing place to take a break.","createdAt":"2026-03-20T08:50:00Z"}]',
  10.7606,
  106.7058,
  'Chào mừng bạn đến với quầy Trà Sữa Vỉa Hè trên phố Vĩnh Khánh. Sau khi thưởng thức các món mặn, nhiều thực khách thường ghé đây để gọi một ly trà sữa hoặc trà trái cây mát lạnh. Thực đơn có nhiều lựa chọn như trà đào, trà tắc, trà sữa truyền thống, trà ô long sữa và các loại topping như trân châu, thạch trái cây, pudding. Không gian nhỏ gọn nhưng thoải mái, rất hợp để ngồi nghỉ chân và tiếp tục khám phá khu phố về đêm.',
  'Welcome to the sidewalk milk tea stall on Vinh Khanh street. After enjoying savory food, many visitors stop here for a chilled milk tea or fruit tea. The menu includes peach tea, kumquat tea, classic milk tea, oolong milk tea, and toppings such as tapioca pearls, fruit jelly, and pudding. The space is compact yet comfortable, making it a pleasant place to rest before continuing your evening food walk.',
  '欢迎来到永庆街的路边奶茶摊。吃完咸食后，很多游客会来这里点一杯冰奶茶或水果茶。菜单包括蜜桃茶、金桔茶、经典奶茶、乌龙奶茶，以及珍珠、果冻、布丁等配料。这里适合稍作休息，再继续夜间美食之旅。',
  'Willkommen beim Strassenstand fur Milchtee in der Vinh Khanh Strasse. Nach herzhaften Gerichten kommen viele Gaste hierher fur einen kalten Milchtee oder Fruchttee. Es gibt Pfirsichtee, Kumquattee, klassischen Milchtee, Oolong Milchtee sowie Tapiokaperlen, Gelee und Pudding. Ein angenehmer Ort fur eine kurze Pause am Abend.'
),
(
  'Xiên Nướng Đêm',
  'Quầy xiên nướng với thịt, rau củ và hải sản tẩm ướp đậm vị, thơm lừng về đêm.',
  'https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800',
  '["https://images.unsplash.com/photo-1529193591184-b1d58069ecdd?w=800","https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800","https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800"]',
  '70 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0946789012',
  '[{"author":"Phúc","rating":5,"comment":"Xiên nướng đậm vị, ăn nóng rất ngon.","createdAt":"2026-03-26T20:10:00Z"},{"author":"Anna","rating":4,"comment":"Tasty grilled skewers and a fun street atmosphere.","createdAt":"2026-03-22T14:05:00Z"}]',
  10.7608,
  106.7060,
  'Chào mừng bạn đến với quầy Xiên Nướng Đêm. Quầy phục vụ nhiều loại xiên như bò cuộn nấm, gà nướng mật ong, xúc xích, đậu bắp, bạch tuộc và các loại viên thả lẩu nướng. Tất cả được tẩm ướp đậm đà rồi nướng trên than hồng, tạo nên mùi thơm hấp dẫn lan khắp vỉa hè. Đây là địa điểm phù hợp để ăn cùng bạn bè, gọi nhiều món khác nhau và vừa ăn vừa tận hưởng không khí nhộn nhịp của khu phố đêm.',
  'Welcome to the night grilled skewer stall. The stall offers many kinds of skewers such as beef rolls with mushrooms, honey grilled chicken, sausages, okra, octopus, and assorted fish balls. Everything is well marinated and grilled over hot charcoal, creating an irresistible aroma along the sidewalk. It is a great stop for sharing different bites with friends while enjoying the lively evening atmosphere.',
  '欢迎来到夜间烤串摊。这里提供牛肉卷蘑菇、蜂蜜烤鸡、香肠、秋葵、章鱼和各种丸子串，食材先腌后烤，香味十足。非常适合和朋友一起边吃边感受热闹的夜市气氛。',
  'Willkommen beim Nachtstand fur Grillspiesse. Hier gibt es viele Sorten wie Rindfleischrollen mit Pilzen, Honighuhn, Wurstchen, Okra, Oktopus und verschiedene Ballchen. Alles wird kraftig mariniert und uber Holzkohle gegrillt. Ein idealer Ort, um mit Freunden verschiedene Kleinigkeiten in lebhafter Abendstimmung zu probieren.'
),
(
  'Bún Thái Hải Sản',
  'Tô bún Thái chua cay với tôm, mực và chả cá, nổi bật bởi nước dùng đậm đà.',
  'https://images.unsplash.com/photo-1555126634-323283e090fa?w=800',
  '["https://images.unsplash.com/photo-1555126634-323283e090fa?w=800","https://images.unsplash.com/photo-1547592180-85f173990554?w=800","https://images.unsplash.com/photo-1482049016688-2d3e1b311543?w=800"]',
  '76 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0957890123',
  '[{"author":"Hà","rating":5,"comment":"Nước dùng chua cay rất bắt vị, topping đầy đặn.","createdAt":"2026-03-24T11:55:00Z"},{"author":"Jason","rating":4,"comment":"A flavorful Thai-style noodle soup with good seafood.","createdAt":"2026-03-18T07:45:00Z"}]',
  10.7610,
  106.7062,
  'Chào mừng bạn đến với quán Bún Thái Hải Sản. Món ăn nổi bật với nước dùng chua cay đậm đà, kết hợp giữa vị hải sản ngọt tự nhiên và hương thơm từ sả, lá chanh, ớt. Trong tô bún có tôm, mực, chả cá, nghêu và rau ăn kèm, tạo cảm giác đầy đặn nhưng vẫn tươi mát. Đây là lựa chọn rất hợp cho những ai thích món nước có vị mạnh, dễ kích thích vị giác và đặc biệt ngon hơn vào buổi tối se mát.',
  'Welcome to the Thai seafood noodle soup shop. This dish stands out with its rich sweet and sour broth, balancing natural seafood flavors with the fragrance of lemongrass, lime leaves, and chili. Each bowl includes shrimp, squid, fish cake, clams, and fresh vegetables, making it hearty yet refreshing. It is an excellent choice for anyone who enjoys bold noodle soups, especially on a cool evening.',
  '欢迎来到泰式海鲜米粉店。这里的汤底酸辣开胃，融合了海鲜的鲜甜和香茅、青柠叶、辣椒的香气。配料有虾、鱿鱼、鱼饼、蛤蜊和蔬菜，内容丰富却不会腻口。喜欢重口味汤粉的人会很喜欢这里。',
  'Willkommen bei Thai Meeresfruchte Nudelsuppe. Die Suppe ist fur ihre kraeftige saure und scharfe Bruhe bekannt, die mit Zitronengras, Limettenblattern und Chili aromatisiert ist. Dazu gibt es Garnelen, Tintenfisch, Fischkuchen, Muscheln und frisches Gemuse. Eine sehr gute Wahl fur Liebhaber intensiver Suppengerichte.'
),
(
  'Kem Cuộn Thái',
  'Quầy kem cuộn mát lạnh với nhiều vị trái cây và topping bắt mắt, phù hợp kết thúc hành trình ẩm thực.',
  'https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=800',
  '["https://images.unsplash.com/photo-1563805042-7684c019e1cb?w=800","https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?w=800","https://images.unsplash.com/photo-1488477181946-6428a0291777?w=800"]',
  '82 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0968901234',
  '[{"author":"Mai","rating":4,"comment":"Kem mịn, vị trái cây dễ ăn và topping đẹp mắt.","createdAt":"2026-03-25T21:30:00Z"},{"author":"Noah","rating":4,"comment":"A fun dessert stop with fresh rolled ice cream.","createdAt":"2026-03-20T15:10:00Z"}]',
  10.7612,
  106.7064,
  'Chào mừng bạn đến với quầy Kem Cuộn Thái. Đây là điểm dừng ngọt ngào để khép lại hành trình ẩm thực trên phố Vĩnh Khánh. Kem được làm trực tiếp trên mặt bàn lạnh, cán mỏng rồi cuộn lại thành từng cuộn nhỏ đẹp mắt. Bạn có thể chọn nhiều hương vị như xoài, dâu, socola, vani hoặc trà xanh, kèm theo trái cây tươi, bánh quy và sốt ngọt. Món kem vừa mát lạnh vừa vui mắt, rất được các bạn trẻ và gia đình yêu thích.',
  'Welcome to the Thai rolled ice cream stall. This is a sweet final stop for your food journey on Vinh Khanh street. The ice cream is prepared live on a freezing plate, spread thin, and rolled into neat curls. You can choose flavors such as mango, strawberry, chocolate, vanilla, or green tea, then add fresh fruit, cookies, and sweet sauces. It is a fun and refreshing dessert loved by young visitors and families alike.',
  '欢迎来到泰式炒冰淇淋摊。这里很适合作为永庆美食之旅的甜蜜收尾。冰淇淋在冰板上现做，铺平后卷成小卷，外观很吸引人。口味有芒果、草莓、巧克力、香草和抹茶，还可以加入水果、饼干和甜酱，清爽又有趣。',
  'Willkommen beim Thai Eisrollen Stand. Dies ist ein suesser Abschluss fur einen kulinarischen Abend auf der Vinh Khanh Strasse. Das Eis wird direkt auf einer kalten Platte zubereitet, ausgerollt und zu kleinen Rollen geformt. Es gibt Sorten wie Mango, Erdbeere, Schokolade, Vanille oder Gruentee, dazu frische Fruchte, Kekse und Sossen. Ein frisches Dessert, das besonders bei jungen Leuten und Familien beliebt ist.'
);

SELECT * FROM Users;
SELECT * FROM Locations;
