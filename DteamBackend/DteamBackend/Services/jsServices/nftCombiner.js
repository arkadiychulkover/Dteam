import sharp from 'sharp';
import fs from 'fs';
import path from 'path';

async function composeNFT(backgroundDir, patternDir, modelDir, outputPath) {
    const backgrounds = fs.readdirSync(backgroundDir).filter(f => f.endsWith('.png'));
    const patterns = fs.readdirSync(patternDir).filter(f => f.endsWith('.png'));
    const models = fs.readdirSync(modelDir).filter(f => f.endsWith('.png'));

    if (!fs.existsSync(outputPath)) {
        fs.mkdirSync(outputPath, { recursive: true });
    }

    for (let i = 0; i < backgrounds.length; i++) {
        const background = path.join(backgroundDir, backgrounds[i]);
        for (let j = 0; j < patterns.length; j++) {
            const pattern = path.join(patternDir, patterns[j]);
            for (let k = 0; k < models.length; k++) {
                const model = path.join(modelDir, models[k]);
                const fileOutputPath = path.join(outputPath, `${i + 1}_${j + 1}_${k + 1}.png`);
                console.log(fileOutputPath, 'fileOutputPath');

                await sharp(background)
                    .composite([
                        { input: pattern },
                        { input: model }
                    ])
                    .toFile(fileOutputPath);
            }
        }
    }

    console.log(`Готово: ${backgrounds.length * patterns.length * models.length} файлов`);
}

const backgroundDir = process.argv[2];
const patternDir = process.argv[3];
const modelDir = process.argv[4];
const outputPath = process.argv[5];

if (!backgroundDir || !patternDir || !modelDir || !outputPath) {
    console.error('Использование: node script.js <фоны> <узоры> <модели> <output>');
    process.exit(1);
}

composeNFT(backgroundDir, patternDir, modelDir, outputPath);