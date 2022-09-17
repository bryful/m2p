# m2p.exe

<b>Visual studio 2022<b>の練習用に作った物です。<br>
ミリ(mm)とピクセル(pixel)をdpiを元に変換します。<br>
<br>
特に説明しなくても大丈夫な用に作ってあります。

# コマンドライン
コマンドラインからリモート操作できます。<br>

* m2p -tocsnter<br>起動していたら画面中央に表示します。
* m2p -exit<br>起動していたら終了させます。
* m2p -mode [mm|dpi|pixel|mult]<br>入力位置を設定します。
* m2p -value "1256"<br> 数値を設定します。
* m2p -clip [mm|dpi|pixel|mult]<br>指定した数値をクリップボードへコピーします。

```
m2p -mode mm -value 100 -mode dpi -value 300 -clip pixel
```
と打ち込むと100mm、解像度300dpi のピクセル値をクリップボードにコピーします

新しく作ったスケルトンの基本機能です。<br>


# Dependency
Visual studio 2022 C#


# License

This software is released under the MIT License, see LICENSE

# Authors

bry-ful(Hiroshi Furuhashi)
twitter:bryful
bryful@gmail.com

