<?php header("Content-Type: image/png"); ?>
<?php

// 画像のサイズ
$im = imagecreate(200, 200); // キャンパスの大きさ
$ix = 100; // 中心のx座標
$iy = 100; // 中心のy座標
$iw = 200; // 画像の横幅
$ih = 200; // 画像の縦幅
$rs = 270; // 開始角度(上：270)
// データ
$data[0] = 1;
$data[1] = 2;

// 色の定義(データの数だけ必要)
$red = imagecolorallocate($im, 255, 0, 0);
$blue = imagecolorallocate($im, 0, 0, 255);
$cd = array($red, $blue);

// 合計値の計算
$length = count($data);
$m = 2;
/*
for ($i = 0; $i <= $length; $i++) {
    $m += $data[$i];
}
 */

// 画像の描画
for ($i = 0; $i < $length; $i++) {
    $st = $rs;
    $rs += $data[$i] / $m * 360;
    if ($rs >= 360)
        $rs -= 360;
    imagefilledarc($im, $ix, $iy, $iw, $ih, $st, $rs, $cd[$i], 4);
}
imagepng($im);
?>