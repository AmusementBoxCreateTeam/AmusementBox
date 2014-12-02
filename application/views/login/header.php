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
            </div>
        </nav>


