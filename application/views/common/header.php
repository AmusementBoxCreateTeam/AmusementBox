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
        <!-- Latest compiled and minified JavaScript -->
        <script src="<?php echo base_url(); ?>html/bootstrap/js/bootstrap.min.js"></script>
        <!-- dispicker -->
        <link rel="stylesheet" href="<?php echo base_url(); ?>html/css/bootstrap-theme.min.css">
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
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">ユーザ<b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li><a href="<?php echo base_url() ?>index.php/user">一覧</a></li>
                            </ul>
                        </li>
                        <!-- ドロップダウンのメニューも配置可能 -->
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">BOX<b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li><a href="">一覧</a></li>
                                <li><a href="">登録</a></li>
                                <!--<li class="divider"></li>!-->
                            </ul>
                        </li>
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">統計<b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li><a href="">リアルタイム</a></li>
                                <li><a href="">ランキング</a></li>
                                <li><a href="">売上</a></li>
                                <!--<li class="divider"></li>!-->
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>


