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
/*
$data[0] = $this->session->userdata['woman'];
$data[1] = $this->session->userdata['man'];
 * 
 */
$data[0] = 12;
$data[1] = 0;

// 色の定義(データの数だけ必要)
$white = imagecolorallocate($im,255,255,255);
$red = imagecolorallocate($im, 255, 0, 0);
$blue = imagecolorallocate($im, 0, 0, 255);
$cd = array($red, $blue);

// 合計値の計算
$length = count($data);

for ($i = 0; $i <= $length; $i++) {
    $m += $data[$i];
}


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