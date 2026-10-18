# Torpedo Search Modes & Air Drop Range (Mk 44/46)

Lets you pick a torpedo's search pattern before launch, and drop air-launched torpedoes from realistic standoff ranges instead of the hardcoded 1 nm.

## Features

- **Search mode submenu** in the torpedo weapon menu, next to search depth and launch speed. The attack preview cone updates to match the selected mode.
- Supported torpedoes: **Mk 44** (air/ship): Straight, Snake, Circle, Spiral. **Mk 46** (air/ship): Straight, Snake, Circle, plus Spiral on the air-launched Mk 46. **Mk 46 Mod 5** (air/ship): Straight, Snake, Circle.
- **Straight that actually runs straight**: in vanilla, a torpedo set to Straight starts circling once it reaches the aim point. With this mod it keeps its heading until it finds a target or runs out of fuel. Once it has acquired a target, it behaves like vanilla: if it loses the target, it circles at the last known position.
- **Air drop range**: helicopters and aircraft release these torpedoes at the torpedo's full range for the selected launch speed, minus a 10% fuel reserve for the search and the terminal sprint. Example: Mk 46 Mod 5 at 30 kn has 8 nm of range, so the release range is 7.2 nm. The vanilla limit is 1 nm.
- **Search speed fix**: while searching, the torpedo holds its launch speed (selected or Auto) instead of jumping to 45 kn and burning through its fuel early. Once it acquires a target, it sprints at full speed.

## Requirements

- [Anchor Chain](https://steamcommunity.com/sharedfiles/filedetails/?id=3380210757). You must also install its **preloader** into BepInEx, following the instructions on the Anchor Chain page. If the preloader isn't installed, Mod Manager shows "The Anchor Chain preloader is not running!" and this mod's code won't load.

## Notes and limitations

- The changes apply to player units only. AI-launched torpedoes behave exactly as in vanilla.
- Released at maximum range, the torpedo has fuel for about 0.5 nm at full speed past the aim point. Against a target running away from the aim point, launch closer.
- The mod replaces 5 ammunition files (`usn_mk44_air/ship`, `usn_mk46_air/ship`, `usn_mk46_mod5_air`) to add the allowed search modes. Other mods that edit the same files will conflict; whichever mod loads last wins.
- The search mode can't be set from the waypoint weapon menu.
- In Straight mode, the aim point marker moves ahead of the torpedo while it runs.
- Made for Sea Power 0.8.x. A game update may need a rebuild. If a patch target isn't found, the mod logs a warning and leaves that part vanilla.

---

# Режимы поиска торпед и дальность сброса (Mk 44/46)

Позволяет выбрать схему поиска торпеды перед пуском и сбрасывать авиационные торпеды с реальной дистанции, а не с захардкоженной 1 мили.

## Возможности

- **Пункт «Режим поиска»** в меню торпеды, рядом с глубиной поиска и скоростью пуска. Конус в превью атаки меняется под выбранный режим.
- Поддерживаемые торпеды: **Mk 44** (авиа/корабельная): прямо, змейкой, круговой, спиралью. **Mk 46** (авиа/корабельная): прямо, змейкой, круговой, плюс спиралью у авиационной. **Mk 46 Mod 5** (авиа/корабельная): прямо, змейкой, круговой.
- **«Прямо» теперь действительно прямо**: в оригинале торпеда в этом режиме начинает кружить, как только доходит до точки прицеливания. С модом она идёт своим курсом, пока не найдёт цель или не кончится топливо. После захвата цели торпеда ведёт себя как в оригинале: если цель потеряна, она кружит в последней известной точке.
- **Дальность сброса с воздуха**: вертолёты и самолёты сбрасывают эти торпеды с полной дальности для выбранной скорости за вычетом 10% запаса топлива на поиск и финальный рывок. Пример: у Mk 46 Mod 5 на 30 узлах дальность 8 миль, значит, сброс идёт с 7,2 мили. В оригинале предел 1 миля.
- **Исправление скорости поиска**: во время поиска торпеда держит скорость пуска (выбранную или «Авто»), а не разгоняется до 45 узлов, сжигая топливо раньше времени. После захвата цели она идёт в атаку на полной скорости.

## Требования

- [Anchor Chain](https://steamcommunity.com/sharedfiles/filedetails/?id=3380210757). Нужно также установить его **preloader** в BepInEx по инструкции со страницы Anchor Chain. Без preloader Mod Manager пишет «The Anchor Chain preloader is not running!», и код этого мода не загрузится.

## Примечания и ограничения

- Изменения действуют только на юниты игрока. Торпеды ИИ работают как в оригинале.
- При сбросе с максимальной дистанции у торпеды после точки прицеливания остаётся топлива примерно на 0,5 мили на полной скорости. По цели, уходящей от точки прицеливания, сбрасывайте ближе.
- Мод заменяет 5 файлов боеприпасов (`usn_mk44_air/ship`, `usn_mk46_air/ship`, `usn_mk46_mod5_air`), чтобы добавить разрешённые режимы поиска. Моды, которые правят те же файлы, будут конфликтовать: побеждает тот, что загружен последним.
- Режим поиска нельзя задать из меню оружия на путевой точке.
- В режиме «Прямо» метка точки прицеливания движется впереди торпеды.
- Сделан под Sea Power 0.8.x. После обновления игры может понадобиться пересборка. Если цель патча не найдена, мод пишет предупреждение в лог и оставляет эту часть без изменений.
