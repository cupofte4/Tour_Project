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
  ('nguyenvana', '123456', 'Nguyễn Văn A', NULL, NULL, NULL, 'user', b'0'),
  ('tranthib', 'password123', 'Trần Thị B', NULL, NULL, NULL, 'user', b'0'),
  ('traveler99', 'dulichvietnam', 'Du Khách 99', NULL, NULL, NULL, 'user', b'0'),
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
  'Chào mừng bạn đến với quán Ốc Vĩnh Khánh! Đây là một trong những quán ốc lâu đời và nổi tiếng nhất tại Phố Ẩm thực Vĩnh Khánh, Quận 4, Thành phố Hồ Chí Minh. Quán phục vụ đa dạng các món ốc tươi ngon như ốc len xào dừa, ốc hương nướng mỡ hành, ốc bươu hấp sả, và nhiều món hải sản đặc sắc khác. Nguyên liệu được chọn lọc tươi sống mỗi ngày, chế biến theo công thức gia truyền với hương vị đậm đà, cay nồng đặc trưng. Hãy dừng chân và thưởng thức bữa ăn đêm tuyệt vời tại đây!',
  'Welcome to Oc Vinh Khanh! This is one of the oldest and most famous snail restaurants on Vinh Khanh Food Street, District 4, Ho Chi Minh City. The restaurant serves a wide variety of fresh snail dishes such as mud creeper snails stir-fried with coconut milk, grilled snails with spring onion butter, steamed apple snails with lemongrass, and many other special seafood dishes. Ingredients are freshly selected every day and prepared using traditional family recipes with rich, spicy flavors. Stop by and enjoy a wonderful night meal here!',
  'Welcome to Oc Vinh Khanh, a famous seafood stop on Vinh Khanh street in District 4.',
  'Willkommen bei Oc Vinh Khanh! Dies ist eines der bekanntesten Schneckenrestaurants in der Vinh Khanh Essensstrasse im Bezirk 4.'
),
(
  'Phá lấu Vĩnh Khánh',
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
  'Welcome to Pha Lau Vinh Khanh, a beloved late-night street food stop in District 4.',
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
  'Chào mừng bạn đến với quán Chè Vĩnh Khánh! Đây là thiên đường chè ngọt giữa lòng phố ẩm thực Vĩnh Khánh. Quán phục vụ hơn 30 loại chè khác nhau như chè Thái, chè khúc bạch, chè bưởi, chè đậu xanh đánh, chè trôi nước, và nhiều loại chè truyền thống Nam Bộ đặc sắc. Mỗi bát chè được chế biến tỉ mỉ từ nguyên liệu tươi ngon, ngọt thanh vừa phải, mát lạnh giải nhiệt hoàn hảo cho những buổi tối Sài Gòn oi bức. Ghé thăm và thử ngay những bát chè thơm ngon tại đây!',
  'Welcome to Che Vinh Khanh! This is a sweet dessert paradise in the heart of Vinh Khanh food street. The shop serves over 30 different types of Vietnamese sweet soups including Thai-style che, che khuc bach, pomelo che, mung bean che, glutinous rice ball che, and many other traditional Southern Vietnamese desserts. Each bowl is carefully prepared from fresh ingredients, perfectly sweetened, and served cold - the perfect refreshment for hot Saigon evenings. Come visit and try the delicious sweet soups here!',
  'Welcome to Che Vinh Khanh, a dessert stop with many sweet soups and chilled treats.',
  'Willkommen bei Che Vinh Khanh! Dies ist ein beliebter Dessertladen mit vielen vietnamesischen Sussspeisen.'
),
(
  'Hải sản nướng',
  'Hải sản nướng tại đây nổi bật với nguyên liệu tươi sống, mùi thơm hấp dẫn và cách chế biến đậm vị.',
  'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800',
  '["https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=800","https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800","https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=800","https://images.unsplash.com/photo-1482049016688-2d3e1b311543?w=800"]',
  '48 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM',
  '0988456789',
  '[{"author":"Quốc Huy","rating":5,"comment":"Hải sản tươi, nướng lên thơm và không khí vỉa hè rất đã.","createdAt":"2026-03-23T17:55:00Z"},{"author":"Maria","rating":4,"comment":"Fresh scallops and crab, lively street-side atmosphere.","createdAt":"2026-03-16T11:25:00Z"}]',
  10.7601,
  106.7053,
  'Chào mừng bạn đến với quán Hải Sản Nướng Vĩnh Khánh! Đây là điểm đến lý tưởng cho những tín đồ hải sản tươi sống tại phố ẩm thực Vĩnh Khánh. Quán nổi tiếng với các món nướng than hoa thơm lừng như tôm hùm nướng bơ tỏi, mực nướng sa tế, sò điệp nướng phô mai, cua rang muối, và ghẹ hấp bia. Hải sản được nhập trực tiếp từ các cảng cá mỗi sáng, đảm bảo độ tươi ngon tuyệt đối. Không khí vỉa hè sôi động, khói nướng thơm ngào ngạt tạo nên trải nghiệm ẩm thực đường phố đặc sắc không thể bỏ qua khi đến Sài Gòn.',
  'Welcome to Vinh Khanh Grilled Seafood! This is the ideal destination for fresh seafood lovers on Vinh Khanh food street. The restaurant is famous for its fragrant charcoal-grilled dishes such as garlic butter grilled lobster, satay grilled squid, cheese grilled scallops, salt and pepper crab, and beer-steamed blue crab. Seafood is imported directly from fishing ports every morning, ensuring absolute freshness. The lively sidewalk atmosphere and aromatic grilling smoke create an unforgettable street food experience not to be missed when visiting Saigon.',
  'Welcome to Vinh Khanh Grilled Seafood, a lively place for fresh charcoal-grilled seafood.',
  'Willkommen bei Vinh Khanh Gegrillten Meeresfruchten! Hier gibt es frische Meeresfruchte und lebhafte Strassenatmosphare.'
);

SELECT * FROM Users;
SELECT * FROM Locations;