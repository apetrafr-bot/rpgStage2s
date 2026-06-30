"""
Découpe une image en tuiles de 5000x5000 pixels maximum.
Usage : python decoupe_image.py <chemin_image>
"""

import sys
import os
from PIL import Image

MAX_SIZE = 2000

def decoupe(image_path):
    img = Image.open(image_path)
    largeur, hauteur = img.size
    print(f"Image originale : {largeur} x {hauteur} px")

    nom, ext = os.path.splitext(image_path)
    dossier  = nom + "_decoupee"
    os.makedirs(dossier, exist_ok=True)

    cols = (largeur + MAX_SIZE - 1) // MAX_SIZE   # nombre de colonnes
    rows = (hauteur + MAX_SIZE - 1) // MAX_SIZE   # nombre de lignes
    print(f"Découpe en {cols} colonne(s) x {rows} ligne(s) = {cols * rows} image(s)")

    for row in range(rows):
        for col in range(cols):
            x1 = col * MAX_SIZE
            y1 = row * MAX_SIZE
            x2 = min(x1 + MAX_SIZE, largeur)
            y2 = min(y1 + MAX_SIZE, hauteur)

            tuile = img.crop((x1, y1, x2, y2))
            nom_fichier = os.path.join(dossier, f"tuile_{row}_{col}{ext}")
            tuile.save(nom_fichier)
            print(f"  Sauvegardé : {nom_fichier}  ({x2-x1} x {y2-y1} px)")

    print(f"\nTerminé ! Images dans : {dossier}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage : python decoupe_image.py <chemin_image>")
        sys.exit(1)
    decoupe(sys.argv[1])
