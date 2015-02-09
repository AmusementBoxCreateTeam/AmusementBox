<!DOCTYPE html>
<!--
To change this license header, choose License Headers in Project Properties.
To change this template file, choose Tools | Templates
and open the template in the editor.
-->
<html>
    <head>
        <meta charset="UTF-8">
        <script src="<?php echo base_url(); ?>html/js/jquery-2.1.1.min.js"></script>
        <!-- bootstrap -->
        <!-- Latest compiled and minified CSS -->
        <link rel="stylesheet" href="<?php echo base_url(); ?>html/bootstrap/css/bootstrap.min.css">
        <!-- Optional theme -->
        <link rel="stylesheet" href="<?php echo base_url(); ?>html/bootstrap/css/bootstrap-theme.min.css">
        <link rel="stylesheet" href="<?php echo base_url(); ?>html/css/base.css">
        <!-- Latest compiled and minified JavaScript -->
        <script src="<?php echo base_url(); ?>html/bootstrap/js/bootstrap.min.js"></script>
        <!-- dispicker -->
        <script src="<?php echo base_url(); ?>html/js/bootstrap-datepicker.js"></script>
        <script src="<?php echo base_url(); ?>html/js/bootstrap-datepicker.ja.js"></script>
        <script src="<?php echo base_url(); ?>html/js/base.js"></script>
        <title>Square管理画面</title>
    </head>
    <body>
        <nav class="navbar navbar-default navbar-inverse" role="navigation">
            <div class="container-fluid">

                <!-- スマートフォンサイズで表示されるメニューボタンとテキスト -->
                <div class="navbar-header">

                    <!-- 
                      navbar-inverse : 黒いスタイルに変更
                      navbar-origin : オリジナルスタイルを当ててみると・・・
                    
                      メニューボタン 
                      
                      data-toggle : ボタンを押したときにNavbarを開かせるために必要
                      data-target : 複数navbarを作成する場合、ボタンとナビを紐づけるために必要
              
                    -->
                    <!-- タイトルなどのテキスト -->
                    <a class="navbar-brand" href="<?php echo base_url() ?>">Square</a>

                </div>

                <!-- グローバルナビの中身 -->
                <div class="collapse navbar-collapse" id="nav-menu-1">
                    <!-- 各ナビゲーションメニュー -->
                    <ul class="nav navbar-nav">

                        <!-- 通常のリンク -->
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">作品概要<b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li><a href="<?php echo base_url() ?>index.php/archive/plan">企画概要・作品内容</a></li>
                                <li><a href="<?php echo base_url() ?>index.php/archive/skill">使用技術</a></li>
                                <!--<li class="divider"></li>!-->
                            </ul>
                        </li>
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">企画書<b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li><a href="<?php echo base_url() ?>html/archive/onesheet.pdf">ワンシート企画書</a></li>
                                <li><a href="<?php echo base_url() ?>html/archive/program.pdf">プログラムイラスト</a></li>
                                <li><a href="<?php echo base_url() ?>index.php/archive/booth">ブースイメージ図</a></li>
                                <!--<li class="divider"></li>!-->
                            </ul>
                        </li>
                        <li class="dropdown">
                            <a href="<?php echo base_url() . 'index.php/archive/account' ?>">会計報告書</b></a>
                        </li>
                        <li class="dropdown">
                            <a href="<?php echo base_url() . 'index.php/archive/impressions' ?>">感想</b></a>
                        </li>
                    </ul>
                    <ul class="nav navbar-brand pull-right">
                        <li><button class="btn-default" onclick="location = '<?php echo base_url() . 'index.php/index/logout' ?>'">ログアウト</button></li>
                    </ul>
                </div>
            </div>
        </nav>

