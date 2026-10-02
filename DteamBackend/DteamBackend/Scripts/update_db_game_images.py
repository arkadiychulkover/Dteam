import sqlite3
import json

db_path = "dteam.db"

main_games = {
    "Cyberpunk 2077": "cyberpunk-2077",
    "The Witcher 3: Wild Hunt": "witcher-3",
    "Elden Ring": "elden-ring",
    "DOOM Eternal": "doom-eternal",
    "Sid Meier's Civilization VI": "civ-6",
    "Forza Horizon 5": "forza-horizon-5",
    "Resident Evil 4 Remake": "resident-evil-4",
    "Baldur's Gate 3": "baldurs-gate-3",
    "Dead Cells": "dead-cells",
    "Subnautica": "subnautica"
}

dlcs = {
    "Cyberpunk 2077: Phantom Liberty": "cyberpunk-phantom-liberty",
    "Cyberpunk 2077: REDmod": "cyberpunk-redmod",
    "The Witcher 3: Hearts of Stone": "witcher-3-hos",
    "The Witcher 3: Blood and Wine": "witcher-3-baw",
    "Elden Ring: Shadow of the Erdtree": "elden-ring-erdtree",
    "Elden Ring: Colosseum & Soundtrack": "elden-ring-colosseum",
    "DOOM Eternal: The Ancient Gods - Part One": "doom-ancient-gods-1",
    "DOOM Eternal: The Ancient Gods - Part Two": "doom-ancient-gods-2",
    "Civilization VI: Gathering Storm": "civ-6-gathering-storm",
    "Civilization VI: Rise and Fall": "civ-6-rise-and-fall",
    "Forza Horizon 5: Hot Wheels": "forza-horizon-5-hot-wheels",
    "Forza Horizon 5: Rally Adventure": "forza-horizon-5-rally",
    "Resident Evil 4: Separate Ways": "re4-separate-ways",
    "Resident Evil 4: Extra DLC Pack": "re4-extra-dlc",
    "Baldur's Gate 3: Digital Deluxe Upgrade": "bg3-deluxe",
    "Baldur's Gate 3: Adventure Companion Pack": "bg3-companion",
    "Dead Cells: Return to Castlevania": "dead-cells-castlevania",
    "Dead Cells: The Bad Seed": "dead-cells-bad-seed",
    "Subnautica: Below Zero": "subnautica-below-zero",
    "Subnautica: Deep Sea Pack": "subnautica-deep-sea"
}

conn = sqlite3.connect(db_path)
cur = conn.cursor()

# 1. Update main games
for title, slug in main_games.items():
    cover = f"/game_images/{slug}_cover.jpg"
    header = f"/game_images/{slug}_header.jpg"
    screenshots = json.dumps([
        f"/game_images/{slug}_ss1.jpg",
        f"/game_images/{slug}_ss2.jpg",
        f"/game_images/{slug}_ss3.jpg"
    ])
    cur.execute("""
        UPDATE Games
        SET CoverImageUrl = ?, HeaderImageUrl = ?, ScreenshotUrls = ?
        WHERE Title = ?
    """, (cover, header, screenshots, title))
    print(f"Updated main game '{title}' -> {cover}, {header}")

# 2. Update DLCs
for title, slug in dlcs.items():
    cover = f"/game_images/{slug}_cover.jpg"
    header = f"/game_images/{slug}_header.jpg"
    screenshots = json.dumps([cover, header])
    cur.execute("""
        UPDATE Games
        SET CoverImageUrl = ?, HeaderImageUrl = ?, ScreenshotUrls = ?
        WHERE Title = ?
    """, (cover, header, screenshots, title))
    print(f"Updated DLC '{title}' -> {cover}, {header}")

# 3. Update CommunityPosts
# Map games to header images
game_headers = {t: f"/game_images/{s}_header.jpg" for t, s in main_games.items()}
for title, header in game_headers.items():
    cur.execute("""
        UPDATE CommunityPosts
        SET GameBannerUrl = ?
        WHERE GameTitle = ?
    """, (header, title))

# Specific community post media mappings
post_media_updates = [
    ("Оновлення 2.13: Генерація кадрів FSR 3 та оптимізація міста", "/game_images/cyberpunk-2077_header.jpg"),
    ("Річниця Phantom Liberty: Понад 5 мільйонів агентів у Песиголові", "/game_images/cyberpunk-phantom-liberty_header.jpg"),
    ("Гайд на найкращий білд Нетраннера у патчі 2.1", "/game_images/cyberpunk-2077_ss1.jpg"),
    ("Нічний Найт-Сіті у неоні та дощі", "/game_images/cyberpunk-2077_ss2.jpg"),
    ("Офіційний редактор модів REDkit вже доступний усім гравцям!", "/game_images/witcher-3_header.jpg"),
    ("Повний гайд по знаходженню гросмейстерських обладунків Школи Кота", "/game_images/witcher-3_ss1.jpg"),
    ("Захід сонця на скелях Ард Скелліг", "/game_images/witcher-3_ss2.jpg"),
    ("Патч 1.14: Ребаланс зброї та балансування босів у Землях Тіні", "/game_images/elden-ring_header.jpg"),
    ("Як підготуватися до битви з Генералом Раданом", "/game_images/elden-ring_ss1.jpg"),
    ("Битва з Маленією без броні на 1 рівні", "/game_images/elden-ring_ss2.jpg"),
    ("Патч 7 вже вийшов: Офіційний менеджер модифікацій та нові кінцівки", "/game_images/baldurs-gate-3_header.jpg"),
    ("Мультиклас Барда та Паладина: Смертоносний Смайт", "/game_images/baldurs-gate-3_ss1.jpg"),
    ("Турнірний сезон у Battlemode та оновлені ефекти вогню", "/game_images/doom-eternal_header.jpg"),
    ("Знищення Мародера за 3 секунди: комбо зброї", "/game_images/doom-eternal_ss1.jpg"),
    ("Фестивальний сезон 'Суперкари Мексики': 4 нових боліди", "/game_images/forza-horizon-5_header.jpg"),
    ("Дрифт на схилах Gran Caldera", "/game_images/forza-horizon-5_ss1.jpg"),
    ("Оновлення режиму The Mercenaries: Нові мапи та персонажі", "/game_images/resident-evil-4_header.jpg"),
    ("Як економити патрони та ресурси на хардкорі", "/game_images/resident-evil-4_ss1.jpg"),
    ("Щомісячний челендж лідерів: Морське панування за Португалію", "/game_images/civ-6_header.jpg"),
    ("Оновлення 'The End is Near': Фінальний патч для безсмертного рогалика", "/game_images/dead-cells_header.jpg"),
    ("Перемога над Дракулою без втрати здоров'я", "/game_images/dead-cells_ss1.jpg"),
    ("Оновлення 2.0: Оновлений рушій та повна підтримка хмарних збережень", "/game_images/subnautica_header.jpg"),
    ("Затишна підводна база у Загубленій річці", "/game_images/subnautica_ss1.jpg"),
]

for title, media_url in post_media_updates:
    cur.execute("""
        UPDATE CommunityPosts
        SET MediaUrl = ?, MediaThumbnailUrl = ?
        WHERE Title = ?
    """, (media_url, media_url, title))

conn.commit()
conn.close()
print("All database game and community images successfully updated!")
